using CitizenFX.Core.Native;
using ELS.FullSync;
using ELS.Light;
using Shared;
using System.Collections.Generic;

namespace ELS.Board
{
    internal class ArrowBoard : IFullSyncComponent
    {
        ILight lights;
        configuration.MISC _misc;
        readonly string _boardType;
        private bool _raise;

        public bool HasBoard { get; private set; }
        internal int BoardDoorIndex { get; set; }
        internal bool BoardRaised { get; set; }

        private void RaiseBoard()
        {
            var vehicle = lights.ElsVehicle.Vehicle;
            if (vehicle != null)
            {
                API.SetVehicleDoorOpen(vehicle.Handle, BoardDoorIndex, false, false);
            }
            BoardRaised = true;
        }

        private void LowerBoard()
        {
            var vehicle = lights.ElsVehicle.Vehicle;
            if (vehicle != null)
            {
                API.SetVehicleDoorShut(vehicle.Handle, BoardDoorIndex, false);
            }
            BoardRaised = false;
        }

        internal bool RaiseBoardNow
        {
            get
            {
                return _raise;
            }
            set
            {
                _raise = value;
                if (RaiseBoardNow)
                {
                    RaiseBoard();
                }
                else
                {
                    LowerBoard();
                }
            }
        }

        public Dictionary<string, object> GetData()
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            dic.Add(DataNames.BoardRaised, RaiseBoardNow);
            return dic;
        }

        public void SetData(IDictionary<string, object> data)
        {
            BoardRaised = (bool)data[DataNames.BoardRaised];
        }

        internal ArrowBoard(ILight light, configuration.MISC misc)
        {
            lights = light;
            _misc = misc;
            _boardType = _misc.ArrowboardType.ToLower();
            RaiseBoardNow = false;
            BoardRaised = false;
            switch (_boardType)
            {
                case "bonnet":
                    BoardDoorIndex = 4;
                    HasBoard = true;
                    break;
                case "boot":
                    BoardDoorIndex = 5;
                    HasBoard = true;
                    break;
                case "boot2":
                    BoardDoorIndex = 6;
                    HasBoard = true;
                    break;
                case "boots":
                    BoardDoorIndex = 5;
                    HasBoard = true;
                    break;
                case "off":
                    BoardDoorIndex = -1;
                    HasBoard = false;
                    break;
                default:
                    HasBoard = false;
                    break;
            }
        }
    }
}
