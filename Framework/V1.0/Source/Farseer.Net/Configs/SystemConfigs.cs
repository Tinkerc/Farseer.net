using System;
using FS.Utils;

namespace FS.Configs
{
    /// <summary> 系统配置文件 </summary>
    public class SystemConfigs : AbsConfigs<SystemConfig> { }

    /// <summary> 系统配置文件 </summary>
    [Serializable]
    public class SystemConfig
    {
        /// <summary> 开启记录数据库执行过程 </summary>
        public bool IsWriteDbLog = false;
    }
}