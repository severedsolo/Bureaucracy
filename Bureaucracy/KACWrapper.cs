using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
// ReSharper disable All

namespace Bureaucracy
{

    ///////////////////////////////////////////////////////////////////////////////////////////
    // BELOW HERE SHOULD NOT BE EDITED - this links to the loaded KAC module without requiring a Hard Dependancy
    ///////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// The Wrapper class to access KAC from another plugin
    /// </summary>
    [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
    public class KacWrapper
    {
        protected static System.Type KacType;
        protected static System.Type KacAlarmType;

        protected static Object ActualKac = null;

        /// <summary>
        /// This is the Kerbal Alarm Clock object
        /// 
        /// SET AFTER INIT
        /// </summary>
        public static Kacapi Kac = null;
        /// <summary>
        /// Whether we found the KerbalAlarmClock assembly in the loadedassemblies. 
        /// 
        /// SET AFTER INIT
        /// </summary>
        public static Boolean AssemblyExists { get { return (KacType != null); } }
        /// <summary>
        /// Whether we managed to hook the running Instance from the assembly. 
        /// 
        /// SET AFTER INIT
        /// </summary>
        public static Boolean InstanceExists { get { return (Kac != null); } }
        /// <summary>
        /// Whether we managed to wrap all the methods/functions from the instance. 
        /// 
        /// SET AFTER INIT
        /// </summary>
        private static Boolean kacWrapped = false;

        /// <summary>
        /// Whether the object has been wrapped and the APIReady flag is set in the real KAC
        /// </summary>
        public static Boolean ApiReady { get { return kacWrapped && Kac.ApiReady && !NeedUpgrade; } }


        public static Boolean NeedUpgrade { get; private set; }

        /// <summary>
        /// This method will set up the KAC object and wrap all the methods/functions
        /// </summary>
        /// <param name="Force">This option will force the Init function to rebind everything</param>
        /// <returns></returns>
        public static Boolean InitKacWrapper()
        {
            //if (!_KACWrapped )
            //{
            //reset the internal objects
            kacWrapped = false;
            ActualKac = null;
            Kac = null;
            LogFormatted("Attempting to Grab KAC Types...");

            //find the base type
            AssemblyLoader.loadedAssemblies.TypeOperation(t =>
            {
                if (t.FullName == "KerbalAlarmClock.KerbalAlarmClock")
                    KacType = t;
            });

            if (KacType == null)
            {
                return false;
            }

            LogFormatted("KAC Version:{0}", KacType.Assembly.GetName().Version.ToString());
            if (KacType.Assembly.GetName().Version.CompareTo(new System.Version(3, 0, 0, 5)) < 0)
            {
                //No TimeEntry or alarmchoice options = need a newer version
                NeedUpgrade = true;
            }
            
            //now the Alarm Type
            KacAlarmType = AssemblyLoader.loadedAssemblies
                .Select(a => a.assembly.GetExportedTypes())
                .SelectMany(t => t)
                .FirstOrDefault(t => t.FullName == "KerbalAlarmClock.KACAlarm");

            if (KacAlarmType == null)
            {
                return false;
            }

            //now grab the running instance
            LogFormatted("Got Assembly Types, grabbing Instance");

            try {
                ActualKac = KacType.GetField("APIInstance", BindingFlags.Public | BindingFlags.Static).GetValue(null);
            } catch (Exception) {
                NeedUpgrade = true;
                LogFormatted("No APIInstance found - most likely you have KAC v2 installed");
                //throw;
            }
            if (ActualKac == null)
            {
                LogFormatted("Failed grabbing Instance");
                return false;
            }

            //If we get this far we can set up the local object and its methods/functions
            LogFormatted("Got Instance, Creating Wrapper Objects");
            Kac = new Kacapi(ActualKac);
            //}
            kacWrapped = true;
            return true;
        }

        /// <summary>
        /// The Type that is an analogue of the real KAC. This lets you access all the API-able properties and Methods of the KAC
        /// </summary>
        public class Kacapi
        {

            internal Kacapi(Object kac)
            {
                //store the actual object
                actualKac = kac;

                //these sections get and store the reflection info and actual objects where required. Later in the properties we then read the values from the actual objects
                //for events we also add a handler
                LogFormatted("Getting APIReady Object");
                apiReadyField = KacType.GetField("APIReady", BindingFlags.Public | BindingFlags.Static);
                LogFormatted("Success: " + (apiReadyField != null).ToString());

                //WORK OUT THE STUFF WE NEED TO HOOK FOR PEOPEL HERE
                LogFormatted("Getting Alarms Object");
                alarmsField = KacType.GetField("alarms", BindingFlags.Public | BindingFlags.Static);
                actualAlarms = alarmsField.GetValue(actualKac);
                LogFormatted("Success: " + (actualAlarms != null).ToString());

                //Events
                LogFormatted("Getting Alarm State Change Event");
                onAlarmStateChangedEvent = KacType.GetEvent("onAlarmStateChanged", BindingFlags.Public | BindingFlags.Instance);
                LogFormatted_DebugOnly("Success: " + (onAlarmStateChangedEvent != null).ToString());
                LogFormatted_DebugOnly("Adding Handler");
                AddHandler(onAlarmStateChangedEvent, actualKac, AlarmStateChanged);

                //Methods
                LogFormatted("Getting Create Method");
                createAlarmMethod = KacType.GetMethod("CreateAlarm", BindingFlags.Public | BindingFlags.Instance);
                LogFormatted_DebugOnly("Success: " + (createAlarmMethod != null).ToString());

                LogFormatted("Getting Delete Method");
                deleteAlarmMethod = KacType.GetMethod("DeleteAlarm", BindingFlags.Public | BindingFlags.Instance);
                LogFormatted_DebugOnly("Success: " + (deleteAlarmMethod != null).ToString());

                LogFormatted("Getting DrawAlarmAction");
                drawAlarmActionChoiceMethod = KacType.GetMethod("DrawAlarmActionChoiceAPI", BindingFlags.Public | BindingFlags.Instance);
                LogFormatted_DebugOnly("Success: " + (drawAlarmActionChoiceMethod != null).ToString());

                //LogFormatted("Getting DrawTimeEntry");
                //DrawTimeEntryMethod = KACType.GetMethod("DrawTimeEntryAPI", BindingFlags.Public | BindingFlags.Instance);
                //LogFormatted_DebugOnly("Success: " + (DrawTimeEntryMethod != null).ToString());

				//Commenting out rubbish lines
                //MethodInfo[] mis = KACType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                //foreach (MethodInfo mi in mis)
                //{
                //    LogFormatted("M:{0}-{1}", mi.Name, mi.DeclaringType);
                //}
            }

            private readonly Object actualKac;

            private readonly FieldInfo apiReadyField;
            /// <summary>
            /// Whether the APIReady flag is set in the real KAC
            /// </summary>
            public Boolean ApiReady
            {
                get
                {
                    if (apiReadyField == null)
                        return false;

                    return (Boolean)apiReadyField.GetValue(null);
                }
            }

            #region Alarms
            private readonly Object actualAlarms;
            private readonly FieldInfo alarmsField;

            /// <summary>
            /// The list of Alarms that are currently active in game
            /// </summary>
            internal KacAlarmList Alarms
            {
                get
                {
                    return ExtractAlarmList(actualAlarms);
                }
            }

            /// <summary>
            /// This converts the KACAlarmList actual object to a new List for consumption
            /// </summary>
            /// <param name="actualAlarmList"></param>
            /// <returns></returns>
            private KacAlarmList ExtractAlarmList(Object actualAlarmList)
            {
                KacAlarmList listToReturn = new KacAlarmList();
                try
                {
                    //iterate each "value" in the dictionary

                    foreach (var item in (IList)actualAlarmList)
                    {
                        KacAlarm r1 = new KacAlarm(item);
                        listToReturn.Add(r1);
                    }
                }
                catch (Exception)
                {
                    //LogFormatted("Arrggg: {0}", ex.Message);
                    //throw ex;
                    //
                }
                return listToReturn;
            }

            #endregion

            #region Events
            /// <summary>
            /// Takes an EventInfo and binds a method to the event firing
            /// </summary>
            /// <param name="event">EventInfo of the event we want to attach to</param>
            /// <param name="kacObject">actual object the eventinfo is gathered from</param>
            /// <param name="handler">Method that we are going to hook to the event</param>
            protected void AddHandler(EventInfo @event, Object kacObject, Action<Object> handler)
            {
                //build a delegate
                Delegate d = Delegate.CreateDelegate(@event.EventHandlerType, handler.Target, handler.Method);
                //get the Events Add method
                MethodInfo addHandler = @event.GetAddMethod();
                //and add the delegate
                addHandler.Invoke(kacObject, new System.Object[] { d });
            }

            //the info about the event;
            private readonly EventInfo onAlarmStateChangedEvent;

            /// <summary>
            /// Event that fires when the State of an Alarm changes
            /// </summary>
            public event AlarmStateChangedHandler OnAlarmStateChanged;
            /// <summary>
            /// Structure of the event delegeate
            /// </summary>
            /// <param name="e"></param>
            public delegate void AlarmStateChangedHandler(AlarmStateChangedEventArgs e);
            /// <summary>
            /// This is the structure that holds the event arguments
            /// </summary>
            public class AlarmStateChangedEventArgs
            {
                public AlarmStateChangedEventArgs(System.Object actualEvent, Kacapi kac)
                {
                    Type type = actualEvent.GetType();
                    this.Alarm = new KacAlarm(type.GetField("alarm").GetValue(actualEvent));
                    this.EventType = (KacAlarm.AlarmStateEventsEnum)type.GetField("eventType").GetValue(actualEvent);

                }

                /// <summary>
                /// Alarm that has had the state change
                /// </summary>
                public KacAlarm Alarm;
                /// <summary>
                /// What the state was before the event
                /// </summary>
                public KacAlarm.AlarmStateEventsEnum EventType;
            }


            /// <summary>
            /// private function that grabs the actual event and fires our wrapped one
            /// </summary>
            /// <param name="actualEvent">actual event from the KAC</param>
            private void AlarmStateChanged(object actualEvent)
            {
                if (OnAlarmStateChanged != null)
                {
                    OnAlarmStateChanged(new AlarmStateChangedEventArgs(actualEvent, this));
                }
            }
            #endregion


            #region Methods
            private readonly MethodInfo createAlarmMethod;

            /// <summary>
            /// Create a new Alarm
            /// </summary>
            /// <param name="alarmType">What type of alarm are we creating</param>
            /// <param name="name">Name of the Alarm for the display</param>
            /// <param name="ut">Universal Time for the alarm</param>
            /// <returns>ID of the newly created alarm</returns>
            internal String CreateAlarm(AlarmTypeEnum alarmType, String name, Double ut)
            {
                return (String)createAlarmMethod.Invoke(actualKac, new System.Object[] { (Int32)alarmType, name, ut });
            }


            private readonly MethodInfo deleteAlarmMethod;
            /// <summary>
            /// Delete an Alarm
            /// </summary>
            /// <param name="alarmId">Unique ID of the alarm</param>
            /// <returns>Success of the deletion</returns>
            internal Boolean DeleteAlarm(String alarmId)
            {
                return (Boolean)deleteAlarmMethod.Invoke(actualKac, new System.Object[] { alarmId });
            }


            private readonly MethodInfo drawAlarmActionChoiceMethod;
            /// <summary>
            /// Delete an Alarm
            /// </summary>
            /// <param name="AlarmID">Unique ID of the alarm</param>
            /// <returns>Success of the deletion</returns>
            internal Boolean DrawAlarmActionChoice(ref AlarmActionEnum choice, String labelText, Int32 labelWidth, Int32 buttonWidth)
            {
                Int32 inValue = (Int32)choice;
                Int32 outValue = (Int32)drawAlarmActionChoiceMethod.Invoke(actualKac, new System.Object[] { inValue, labelText, labelWidth, buttonWidth });

                choice = (AlarmActionEnum)outValue;
                return (inValue != outValue);
            }

            //Remmed out due to it borking window layout
            //private MethodInfo DrawTimeEntryMethod;
            ///// <summary>
            ///// Delete an Alarm
            ///// </summary>
            ///// <param name="AlarmID">Unique ID of the alarm</param>
            ///// <returns>Success of the deletion</returns>

            //internal Boolean DrawTimeEntry(ref Double Time, TimeEntryPrecisionEnum Prec, String LabelText, Int32 LabelWidth)
            //{
            //    Double InValue = Time;
            //    Double OutValue = (Double)DrawTimeEntryMethod.Invoke(actualKAC, new System.Object[] { InValue, (Int32)Prec, LabelText, LabelWidth });

            //    Time = OutValue;
            //    return (InValue != OutValue);
            //}


            #endregion

            public class KacAlarm
            {
                internal KacAlarm(Object a)
                {
                    actualAlarm = a;
                    vesselIdField = KacAlarmType.GetField("VesselID");
                    idField = KacAlarmType.GetField("ID");
                    nameField = KacAlarmType.GetField("Name");
                    notesField = KacAlarmType.GetField("Notes");
                    alarmTypeField = KacAlarmType.GetField("TypeOfAlarm");
                    alarmTimeProperty = KacAlarmType.GetProperty("AlarmTimeUT");
                    alarmMarginField = KacAlarmType.GetField("AlarmMarginSecs");
                    alarmActionField = KacAlarmType.GetField("AlarmAction");
                    remainingField = KacAlarmType.GetField("Remaining");

                    xferOriginBodyNameField = KacAlarmType.GetField("XferOriginBodyName");
                    //LogFormatted("XFEROrigin:{0}", XferOriginBodyNameField == null);
                    xferTargetBodyNameField = KacAlarmType.GetField("XferTargetBodyName");

                    repeatAlarmField = KacAlarmType.GetField("RepeatAlarm");
                    repeatAlarmPeriodProperty = KacAlarmType.GetProperty("RepeatAlarmPeriodUT");

                    //PropertyInfo[] pis = KACAlarmType.GetProperties();
                    //foreach (PropertyInfo pi in pis)
                    //{
                    //    LogFormatted("P:{0}-{1}", pi.Name, pi.DeclaringType);
                    //}
                    //FieldInfo[] fis = KACAlarmType.GetFields();
                    //foreach (FieldInfo fi in fis)
                    //{
                    //    LogFormatted("F:{0}-{1}", fi.Name, fi.DeclaringType);
                    //}
                }
                private readonly Object actualAlarm;

                private readonly FieldInfo vesselIdField;
                /// <summary>
                /// Unique Identifier of the Vessel that the alarm is attached to
                /// </summary>
                public String VesselId
                {
                    get { return (String)vesselIdField.GetValue(actualAlarm); }
                    set { vesselIdField.SetValue(actualAlarm, value); }
                }

                private readonly FieldInfo idField;
                /// <summary>
                /// Unique Identifier of this alarm
                /// </summary>
                public String Id
                {
                    get { return (String)idField.GetValue(actualAlarm); }
                }

                private readonly FieldInfo nameField;
                /// <summary>
                /// Short Text Name for the Alarm
                /// </summary>
                public String Name
                {
                    get { return (String)nameField.GetValue(actualAlarm); }
                    set { nameField.SetValue(actualAlarm, value); }
                }

                private readonly FieldInfo notesField;
                /// <summary>
                /// Longer Text Description for the Alarm
                /// </summary>
                public String Notes
                {
                    get { return (String)notesField.GetValue(actualAlarm); }
                    set { notesField.SetValue(actualAlarm, value); }
                }

                private readonly FieldInfo xferOriginBodyNameField;
                /// <summary>
                /// Name of the origin body for a transfer
                /// </summary>
                public String XferOriginBodyName
                {
                    get { return (String)xferOriginBodyNameField.GetValue(actualAlarm); }
                    set { xferOriginBodyNameField.SetValue(actualAlarm, value); }
                }

                private readonly FieldInfo xferTargetBodyNameField;
                /// <summary>
                /// Name of the destination body for a transfer
                /// </summary>
                public String XferTargetBodyName
                {
                    get { return (String)xferTargetBodyNameField.GetValue(actualAlarm); }
                    set { xferTargetBodyNameField.SetValue(actualAlarm, value); }
                }
                
                private readonly FieldInfo alarmTypeField;
                /// <summary>
                /// What type of Alarm is this - affects icon displayed and some calc options
                /// </summary>
                public AlarmTypeEnum AlarmType { get { return (AlarmTypeEnum)alarmTypeField.GetValue(actualAlarm); } }

                private readonly PropertyInfo alarmTimeProperty;
                /// <summary>
                /// In game UT value of the alarm
                /// </summary>
                public Double AlarmTime
                {
                    get { return (Double)alarmTimeProperty.GetValue(actualAlarm,null); }
                    set { alarmTimeProperty.SetValue(actualAlarm, value, null); }
                }

                private readonly FieldInfo alarmMarginField;
                /// <summary>
                /// In game seconds the alarm will fire before the event it is for
                /// </summary>
                public Double AlarmMargin
                {
                    get { return (Double)alarmMarginField.GetValue(actualAlarm); }
                    set { alarmMarginField.SetValue(actualAlarm, value); }
                }

                private readonly FieldInfo alarmActionField;
                /// <summary>
                /// What should the Alarm Clock do when the alarm fires
                /// </summary>
                public AlarmActionEnum AlarmAction
                {
                    get { return (AlarmActionEnum)alarmActionField.GetValue(actualAlarm); }
                    set { alarmActionField.SetValue(actualAlarm, (Int32)value); }
                }

                private readonly FieldInfo remainingField;
                /// <summary>
                /// How much Game time is left before the alarm fires
                /// </summary>
                public Double Remaining { get { return (Double)remainingField.GetValue(actualAlarm); } }


                private readonly FieldInfo repeatAlarmField;
                /// <summary>
                /// Whether the alarm will be repeated after it fires
                /// </summary>
                public Boolean RepeatAlarm
                {
                    get { return (Boolean)repeatAlarmField.GetValue(actualAlarm); }
                    set { repeatAlarmField.SetValue(actualAlarm, value); }
                }
                private readonly PropertyInfo repeatAlarmPeriodProperty;
                /// <summary>
                /// Value in Seconds after which the alarm will repeat
                /// </summary>
                public Double RepeatAlarmPeriod
                {
                    get
                    {
                        try { return (Double)repeatAlarmPeriodProperty.GetValue(actualAlarm, null); }
                        catch (Exception) { return 0; }
                    }
                    set { repeatAlarmPeriodProperty.SetValue(actualAlarm, value, null); }
                }

                public enum AlarmStateEventsEnum
                {
                    Created,
                    Triggered,
                    Closed,
                    Deleted,
                }
            }

            public enum AlarmTypeEnum
            {
                Raw,
                Maneuver,
                ManeuverAuto,
                Apoapsis,
                Periapsis,
                AscendingNode,
                DescendingNode,
                LaunchRendevous,
                Closest,
                SoiChange,
                SoiChangeAuto,
                Transfer,
                TransferModelled,
                Distance,
                Crew,
                EarthTime,
                Contract,
                ContractAuto,
                ScienceLab
            }

            public enum AlarmActionEnum
            {
                [Description("Do Nothing-Delete When Past")]        DoNothingDeleteWhenPassed,
                [Description("Do Nothing")]                         DoNothing,
                [Description("Message Only-No Affect on warp")]     MessageOnly,
                [Description("Kill Warp Only-No Message")]          KillWarpOnly,
                [Description("Kill Warp and Message")]              KillWarp,
                [Description("Pause Game and Message")]             PauseGame,
            }

            public enum TimeEntryPrecisionEnum
            {
                Seconds = 0,
                Minutes = 1,
                Hours = 2,
                Days = 3,
                Years = 4
            }

            public class KacAlarmList : List<KacAlarm>
            {

            }
        }
        #region Logging Stuff
        /// <summary>
        /// Some Structured logging to the debug file - ONLY RUNS WHEN DLL COMPILED IN DEBUG MODE
        /// </summary>
        /// <param name="message">Text to be printed - can be formatted as per String.format</param>
        /// <param name="strParams">Objects to feed into a String.format</param>
        [System.Diagnostics.Conditional("DEBUG")]
        internal static void LogFormatted_DebugOnly(String message, params Object[] strParams)
        {
            LogFormatted(message, strParams);
        }

        /// <summary>
        /// Some Structured logging to the debug file
        /// </summary>
        /// <param name="message">Text to be printed - can be formatted as per String.format</param>
        /// <param name="strParams">Objects to feed into a String.format</param>
        internal static void LogFormatted(String message, params Object[] strParams)
        {
            message = String.Format(message, strParams);
            String strMessageLine = String.Format("{0},{2}-{3},{1}",
                DateTime.Now, message, System.Reflection.Assembly.GetExecutingAssembly().GetName().Name,
                System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            UnityEngine.Debug.Log(strMessageLine);
        }
        #endregion
    }
}