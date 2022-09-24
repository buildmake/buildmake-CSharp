using System;
using System.Collections.Generic;

namespace buildmake.Command
{
    class Command
    {
        private String name;

        public Command()
        {
           
        }
        public void SetName(String name)
        {
            this.name = name;
        }

        public String GetName()
        {
            return this.name;
        }

        public List<Command> GetCommands()
        {
            List<Command> commands = new List<Command>();

            String[] commandLineArray = Environment.GetCommandLineArgs();

            for (int i = 1; i < commandLineArray.Length; i = i + 2)
            {
               switch(commandLineArray[i])
                {
                    case "--help":
                    case "-H":
                        if(true) { 
                            Command command = new ShowHelpCommand();
                            commands.Add(command);
                        }
                        break;

                    case "-S":
                    case "--source":
                        if (true)
                        {
                            Command command = new SourceCommand(commandLineArray[i + 1]);
                            commands.Add(command);
                        }
                        break;

                    case "-B":
                    case "--build":
                        if (true)
                        {
                            Command command = new BuildCommand(commandLineArray[i + 1]);
                            commands.Add(command);
                        }
                        break;
                    case "-G":
                    case "--generator":
                        if (true)
                        {
                            Command command = new GeneratorCommand(commandLineArray[i + 1]);
                            commands.Add(command);
                        }
                        break;
                }
            }

            if(commands.Count == 0)
            {
                commands.Add(new ShowHelpCommand());
            }

            return commands;
        }
    }
}
