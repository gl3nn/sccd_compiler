using System;
using System.Linq;

namespace csharp_sccd_compiler
{
    public abstract class CodeGenerator : Visitor
    {
        protected Platform current_platform;
        protected Platform[] supported_platforms;
        protected FileOutputer output_file;

        public enum Platform {
            THREADS,
            GAMELOOP
        };

        private enum IndentType {
            NOT_SET,
            SPACES,
            TABS
        }

        public CodeGenerator()
        {

        }

        public bool generate(ClassDiagram class_diagram, string output_file_path, Platform current_platform)
        {
            this.current_platform = current_platform;
            if (! this.supported_platforms.Contains(this.current_platform))
            {
                Logger.displayError("Unsupported platform.");
                return false;
            }
            try
            {
                this.output_file = new FileOutputer(output_file_path);
                class_diagram.accept(this);
            }
            finally
            {
                this.output_file.close();
            }
            return true;
        }

        protected void writeCorrectIndent(string code)
        {
            string[] lines = code.Split('\n');

            int index = 0;
            while (lines.Length > index && lines[index].Trim() == "")
                index += 1;

            if (index >= lines.Length)
                return;

            //first index where valid code is present
            int to_strip_length = lines[index].TrimEnd().Length - lines[index].Trim().Length;
            IndentType indent_type = IndentType.NOT_SET;

            foreach (string line in lines)
            {
                string strip_part = line.Substring(0, to_strip_length);

                if( (strip_part.Contains('\t') && strip_part.Contains(' '))         ||
                    (indent_type == IndentType.SPACES && strip_part.Contains('\t')) ||
                    (indent_type == IndentType.TABS && strip_part.Contains(' '))   
                )
                   throw new CodeBlockException("Mixed tab and space indentation!");

                if (indent_type == IndentType.NOT_SET)
                {
                    if (strip_part.Contains(' '))
                        indent_type = IndentType.SPACES;
                    else if (strip_part.Contains('\t'))
                        indent_type = IndentType.TABS;
                }
                this.output_file.write(line.Substring(to_strip_length));
            }
        }
    }
}