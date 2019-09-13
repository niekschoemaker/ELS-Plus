using ELS.FullSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELS.Light
{
    partial class Lights : IFullSyncComponent
    {
        public Dictionary<string, object> GetData()
        {

            var dic = new Dictionary<string, object>();
            var prm = new Dictionary<int, object>();
            foreach (Extra.Extra e in _extras.PRML.Values)
            {
                prm.Add(e.Id, e.GetData());
#if DEBUG
                Utils.DebugWriteLine($"Added {e.Id} to prml sync data");
#endif
            }
            var sec = new Dictionary<int, object>();
            foreach (Extra.Extra e in _extras.SECL.Values)
            {
                sec.Add(e.Id, e.GetData());
#if DEBUG
                Utils.DebugWriteLine($"Added {e.Id} to secl sync data");
#endif

            }
            var wrn = new Dictionary<int, object>();
            foreach (Extra.Extra e in _extras.WRNL.Values)
            {
                wrn.Add(e.Id, e.GetData());
#if DEBUG
                Utils.DebugWriteLine($"Added {e.Id} to wrnl sync data");
#endif

            }
            if (prm != null && prm.Count > 0)
            {
                dic.Add("PRML", prm);
#if DEBUG
                Utils.DebugWriteLine($"added PRML data");
#endif
            }
            if (sec != null && sec.Count > 0)
            {
                dic.Add("SECL", sec);
#if DEBUG
                Utils.DebugWriteLine($"added secl data");
#endif

            }
            if (wrn != null && wrn.Count > 0)
            {
                dic.Add("WRNL", wrn);
#if DEBUG
                Utils.DebugWriteLine($"added wrnl data");
#endif

            }
            if (_extras.SBRN != null)
            {
                dic.Add("SBRN", _extras.SBRN.GetData());
#if DEBUG
                Utils.DebugWriteLine($"added SBRN data");
#endif

            }
            if (_extras.SCL != null)
            {
                dic.Add("SCL", _extras.SCL.GetData());
#if DEBUG
                Utils.DebugWriteLine($"added SCL data");
#endif
            }
            if (_extras.TDL != null)
            {
                dic.Add("TDL", _extras.TDL.GetData());
#if DEBUG
                Utils.DebugWriteLine($"added TDL data");
#endif

            }
            dic.Add("BRD", _extras.BRD.GetData());
            dic.Add("PrmPatt", CurrentPrmPattern);
            dic.Add("SecPatt", CurrentSecPattern);
            dic.Add("WrnPatt", CurrentWrnPattern);
            dic.Add("stage", _stage.CurrentStage);
            if (spotLight != null)
            {
                dic.Add("spotlight", spotLight.GetData());
            }
            if (scene != null)
            {
                dic.Add("scene", scene.GetData());
            }
            return dic;
        }

        public void SetData(IDictionary<string, object> data)
        {



            if (data.ContainsKey("PRML"))
            {
#if DEBUG
                Utils.DebugWriteLine($"Got PRML data");
#endif
                IDictionary<string, object> prm = (IDictionary<string, object>)data["PRML"];
                foreach (Extra.Extra e in _extras.PRML.Values)
                {
                    e.SetData((IDictionary<string, object>)prm[$"{e.Id}"]);
#if DEBUG
                    Utils.DebugWriteLine($"Added {e.Id} from prml sync data");
#endif
                }
            }
            if (data.ContainsKey("SECL"))
            {
#if DEBUG
                Utils.DebugWriteLine($"Got SECL DAta");
#endif
                IDictionary<string, object> sec = (IDictionary<string, object>)data["SECL"];
                foreach (Extra.Extra e in _extras.SECL.Values)
                {
                    e.SetData((IDictionary<string, object>)sec[$"{e.Id}"]);
#if DEBUG
                    Utils.DebugWriteLine($"Added {e.Id} from secl sync data");
#endif

                }
            }
            if (data.ContainsKey("WRNL"))
            {
#if DEBUG
                Utils.DebugWriteLine($"Got WRNL data");
#endif
                IDictionary<string, object> wrn = (IDictionary<string, object>)data["WRNL"];
                foreach (Extra.Extra e in _extras.WRNL.Values)
                {
                    e.SetData((IDictionary<string, object>)wrn[$"{e.Id}"]);
#if DEBUG
                    Utils.DebugWriteLine($"Added {e.Id} from wrnl sync data");
#endif
                }
            }
            try
            {
                if (data.ContainsKey("SBRN"))
                {
                    _extras.SBRN.SetData((IDictionary<string, object>)data["SBRN"]);
#if DEBUG
                    Utils.DebugWriteLine($"Added SBRN from sync data");
#endif
                }
            }
            catch (Exception e)
            {
#if DEBUG
                Utils.DebugWriteLine($"SBRN error: {e.Message}");
#endif
            }
            try
            {
                if (data.ContainsKey("SCL"))
                {
                    _extras.SCL.SetData((IDictionary<string, object>)data["SCL"]);
#if DEBUG
                    Utils.DebugWriteLine($"Added SCL from sync data");
#endif
                }
            }
            catch (Exception e)
            {
#if DEBUG
                Utils.DebugWriteLine($"SCL error: {e.Message}");
#endif
            }
            try
            {
                if (data.ContainsKey("TDL"))
                {
                    _extras.TDL.SetData((IDictionary<string, object>)data["TDL"]);
#if DEBUG
                    Utils.DebugWriteLine($"Added TDL from sync data");
#endif
                }
            }
            catch (Exception e)
            {
#if DEBUG
                Utils.DebugWriteLine($"TDL error: {e.Message}");
#endif
            }
            try
            {
                if (data.ContainsKey("BRD"))
                {
#if DEBUG
                    Utils.DebugWriteLine($"Got BRD from sync data");
#endif
                    _extras.BRD.SetData((IDictionary<string, object>)data["BRD"]);
#if DEBUG
                    Utils.DebugWriteLine($"Added BRD from sync data");
#endif
                }
            }
            catch (Exception e)
            {
#if DEBUG
                Utils.DebugWriteLine($"BRD error: {e.Message}");
#endif
            }
            if (data.ContainsKey("PrmPatt"))
            {
                CurrentPrmPattern = int.Parse(data["PrmPatt"].ToString());
#if DEBUG
                Utils.DebugWriteLine($"Added PrmPatt from sync data");
#endif
            }
            if (data.ContainsKey("SecPatt"))
            {
                CurrentSecPattern = int.Parse(data["SecPatt"].ToString());
#if DEBUG
                Utils.DebugWriteLine($"Added SecPatt from sync data");
#endif
            }
            if (data.ContainsKey("WrnPatt"))
            {
                CurrentWrnPattern = int.Parse(data["WrnPatt"].ToString());
#if DEBUG
                Utils.DebugWriteLine($"Added WrnPatt from sync data");
#endif
            }
            if (data.ContainsKey("stage"))
            {
                _stage.SetStage(int.Parse(data["stage"].ToString()));
#if DEBUG
                Utils.DebugWriteLine($"Added stage from sync data");
#endif
            }
            if (spotLight != null)
            {
                spotLight.SetData((IDictionary<string, object>)data["spotlight"]);
            }
            if (scene != null)
            {
                scene.SetData((IDictionary<string, object>)data["scene"]);
            }
        }
    }
}
