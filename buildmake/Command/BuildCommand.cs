using System;

namespace buildmake.Command
{

    class BuildCommand : Command
    {
        private String dir;

        public BuildCommand(string dir)
        {
            this.dir = dir;
        }

        public void SetDir(string dir)
        {
            this.dir = dir;
        }

        public String GetDir()
        {
            return this.dir;
        }
    }
}
