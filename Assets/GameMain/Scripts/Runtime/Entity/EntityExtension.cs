using UnityGameFramework.Runtime;

namespace GameMain.Runtime
{
    public static class EntityExtension
    {
        private static int s_SerialId = 0;
        private static readonly object _serialIdLock = new object();
        public static int GenerateSerialId(this EntityComponent entityComponent)
        {
            lock (_serialIdLock)
            {
                return --s_SerialId;
            }
        }
    }
}
