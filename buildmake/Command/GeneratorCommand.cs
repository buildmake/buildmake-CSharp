using System;

namespace buildmake.Command
{

    class GeneratorCommand : Command
    {
        private String generator;

        public GeneratorCommand(string generator)
        {
            this.generator = generator;
        }

        public void SetGenerator(string dir)
        {
            this.generator = dir;
        }

        public String GetGenerator()
        {
            return this.generator;
        }
    }
}
