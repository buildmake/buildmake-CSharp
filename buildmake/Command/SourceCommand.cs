using System;

namespace buildmake.Command
{
    class SourceCommand : Command
    {
        private String dir;

        public SourceCommand(string dir)
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
