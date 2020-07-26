using Sandbox.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        //IMyShipConnector _ship_dock;
        IMyShipConnector _ground_dock;
        //IMyRemoteControl _remote_control;
        Debug _debug;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
            _debug = new Debug(this);

            //_ship_dock = GridTerminalSystem.GetBlockWithName("DRN-M: Connector") as IMyShipConnector;
            _ground_dock = GridTerminalSystem.GetBlockWithName("STN: Connector") as IMyShipConnector;
            //_remote_control = GridTerminalSystem.GetBlockWithName("DRN-M: Remote Control") as IMyRemoteControl;
        }

        public void Save()
        {
        }

        public void Main(string argument, UpdateType updateSource)
        {
            var grid_orient = _ground_dock.CubeGrid.GridIntegerToWorld(new Vector3I());
            _debug.WriteLine($"Position: {_ground_dock.Position}");


            MatrixD g2w = GetGrid2WorldTransform(_ground_dock.CubeGrid);
            Vector3D gridPos = (new Vector3D(_ground_dock.Min + _ground_dock.Max)) / 2.0; //( .Position is a problem for even size blocks)
            Vector3D calcPos = Vector3D.Transform(gridPos, ref g2w);
            double err = (_ground_dock.GetPosition() - calcPos).Length();

            //Find the world "forward" vector for the block
            MatrixD b2w = GetBlock2WorldTransform(_ground_dock);
            Vector3D fwd = b2w.Forward;
            fwd.Normalize(); //(Need to normalize because the above matrices are scaled by grid size)

            _debug.WriteLine($"{_ground_dock.CustomName}: Error={err:f3} fwd={fwd.X:f3},{fwd.Y:f3},{fwd.Z:f3}");

            //_display.WriteText($"{grid_orient}");
        }

        MatrixD GetGrid2WorldTransform(IMyCubeGrid grid)
        {
            Vector3D origin = grid.GridIntegerToWorld(new Vector3I(0, 0, 0));
            Vector3D plusY = grid.GridIntegerToWorld(new Vector3I(0, 1, 0)) - origin;
            Vector3D plusZ = grid.GridIntegerToWorld(new Vector3I(0, 0, 1)) - origin;
            return MatrixD.CreateScale(grid.GridSize) * MatrixD.CreateWorld(origin, -plusZ, plusY);
        }

        MatrixD GetBlock2WorldTransform(IMyCubeBlock blk)
        {
            Matrix blk2grid;
            blk.Orientation.GetMatrix(out blk2grid);
            return blk2grid *
                   MatrixD.CreateTranslation(((Vector3D)new Vector3D(blk.Min + blk.Max)) / 2.0) *
                   GetGrid2WorldTransform(blk.CubeGrid);
        }

    }
}
