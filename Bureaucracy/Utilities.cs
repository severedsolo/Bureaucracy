
namespace Bureaucracy
{
    public class Utilities
    {
        public static Utilities Instance;

        public Utilities()
        {
            Instance = this;
        }
        public void NewKacAlarm(string alarmName, double alarmTime)
        {
            if (!Bureaucracy.Instance.settings.StopTimeWarp) return;
            if (!KacWrapper.AssemblyExists) return;
            if (!KacWrapper.ApiReady) return;
            KacWrapper.Kac.CreateAlarm(KacWrapper.Kacapi.AlarmTypeEnum.Raw, alarmName, alarmTime);
        }
    }
}