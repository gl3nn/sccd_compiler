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

        public CodeGenerator(Platform[] supported_platforms)
        {
            this.supported_platforms = supported_platforms;
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

            int begin_index = 0;
            while (begin_index < lines.Length && lines[begin_index].Trim() == "")
                begin_index += 1;

            if (begin_index >= lines.Length)
                return;

            int end_index = lines.Length - 1;
            while (end_index > begin_index && lines[end_index].Trim() == "")
            {
                end_index -= 1;
            }

            //first index where valid code is present
            int to_strip_length = lines[begin_index].TrimEnd().Length - lines[begin_index].Trim().Length;
            IndentType indent_type = IndentType.NOT_SET;

            for(int index = begin_index; index <= end_index; ++index)
            {
				string strip_part = lines[index].Substring (0, to_strip_length);

				if ((strip_part.Contains ('\t') && strip_part.Contains (' ')) ||
					(indent_type == IndentType.SPACES && strip_part.Contains ('\t')) ||
					(indent_type == IndentType.TABS && strip_part.Contains (' '))   
                )
					throw new CodeBlockException ("Mixed tab and space indentation!");

				if (indent_type == IndentType.NOT_SET) {
					if (strip_part.Contains (' '))
						indent_type = IndentType.SPACES;
					else if (strip_part.Contains ('\t'))
						indent_type = IndentType.TABS;
				}
				this.output_file.write (lines[index].Substring (to_strip_length));
            }
        }
    }
}