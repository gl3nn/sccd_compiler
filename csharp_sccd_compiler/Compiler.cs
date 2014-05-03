using System;

namespace csharp_sccd_compiler
{
    public class Compiler
    {

        public static void generate(string input_file, string output_file, CodeGenerator.Platform platform)
        {
            ClassDiagram class_diagram = createAST(input_file);
            generateFromAST(class_diagram, output_file, platform);
        }
              
        public static ClassDiagram createAST(string input_file)
        {
            ClassDiagram cd = new ClassDiagram(input_file); //create AST
            (new StateLinker()).visit(cd); //visitor fixing state references
            (new PathCalculator()).visit(cd); //visitor calculating paths
            return cd;
        }
            
        public static void generateFromAST(ClassDiagram class_diagram, string output_file, CodeGenerator.Platform platform)
        {
            if ((new CSharpGenerator()).generate(class_diagram, output_file, platform))
                Logger.displayInfo("The following classes <" + string.Join(", ", class_diagram.class_names) + "> have been exported to the following file: " + output_file);
        }
    }
}

