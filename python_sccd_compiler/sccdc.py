import argparse
import os
from python_generator import PythonGenerator
from csharp_generator import CSharpGenerator
from utils import Logger
from code_generation import Languages, Protocols
from state_linker import StateLinker
from path_calculator import PathCalculator
from constructs import ClassDiagram
from compiler_exceptions import CompilerException

   
def generate(input_file, output_file, target_language, protocol):
    class_diagram = createAST(input_file)
    generateFromAST(class_diagram, output_file, target_language, protocol)
      
def createAST(input_file):
    cd = ClassDiagram(input_file) #create AST
    StateLinker().visit(cd) #visitor fixing state references
    PathCalculator().visit(cd) #visitor calculating paths
    return cd
    
def generateFromAST(class_diagram, output_file, target_language, protocol):
    succesfull_generation = False
    if target_language == Languages.Python :
        succesfull_generation = PythonGenerator(class_diagram, output_file, protocol).generate()
    elif target_language == Languages.CSharp:
        succesfull_generation = CSharpGenerator(class_diagram, output_file, protocol).generate()
    # let user know ALL classes have been processed and loaded
    if succesfull_generation :
        Logger.showInfo("The following classes <" + ", ".join(class_diagram.class_names) + "> have been exported to the following file: " + output_file)
        
def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('input', help='The path to the XML file to be compiled.')
    parser.add_argument('-o', '--output', type=str, help='The path to the generated code. Defaults to the same name as the input file but with matching extension.')
    parser.add_argument('-v', '--verbose', type=int, help='0 = no output, 1 = only show warnings, 2 = show all output. Defaults to 2.', default = 2)
    parser.add_argument('-p', '--platform', type=str, help="Let the compiled code run on top of threads or gameloop. The default is threads.")
    parser.add_argument('-l', '--language', type=str, help='Target language, either "csharp" or "python". Defaults to the latter.')
    
    args = vars(parser.parse_args())
    
    #Set verbose
    if args['verbose'] :
        if args['verbose'] in [0,1,2] :
            Logger.verbose = args['verbose']
        else :
            Logger.showError("Invalid verbose argument.")
    else :
        Logger.verbose = 2

    #Set source file
    source = args['input'].lower()
    if not source.endswith(".xml") :
        Logger.showError("Input file not valid.")
        return
    
    #Set target language
    if args['language'] :
        args['language'] = args['language'].lower()

        if args['language'] == "csharp" or args['language'] == "c#" :
            target_language = Languages.CSharp
        elif args['language'] == "python" :
            target_language = Languages.Python
        else :
            Logger.showError("Invalid language.")
    else :
        target_language = Languages.Python  
        
    #Set output file
    if args['output'] :
        output = args['output'].lower()
        if target_language == Languages.Python and not output.endswith(".py") :
            Logger.showError('Output file should end in ".py".')
            return
        elif target_language == Languages.CSharp and not output.endswith(".cs") :
            Logger.showError('Output file should end in ".cs".')
            return
    else :
        output = os.path.splitext(os.path.split(source)[1])[0] 
        if target_language == Languages.Python :
            output += ".py"
        elif target_language == Languages.CSharp :
            output += ".cs"
        
    #Set protocol    
    if args['platform'] :
        args['platform'] = args['platform'].lower()
        if args['platform'] == "threads" :
            protocol = Protocols.Threads
        elif args['platform'] == "gameloop" :
            protocol = Protocols.GameLoop
        else :
            Logger.showError("Invalid platform.")
            return          
    else :
        protocol = Protocols.Threads
        
    #Compile    
    try :
        generate(source, output, target_language, protocol)
    except CompilerException as exception :
        print exception

if __name__ == "__main__":
    main()


