Statecharts and Class Diagram Compiler
======================================

Usage
-------------
Manual for the compiler written in Python :
```sh
$python sccdc.py --help
usage: sccdc.py [-h] [-o OUTPUT] [-v VERBOSE] [-p PROTOCOL] [-l LANGUAGE]
                input

positional arguments:
  input                 The path to the XML file to be compiled.

optional arguments:
  -h, --help            show this help message and exit
  -o OUTPUT, --output OUTPUT
                        The path to the generated code. Defaults to the same
                        name as the input file but with matching extension.
  -v VERBOSE, --verbose VERBOSE
                        2 = all output; 1 = only warnings and errors; 0 = only
                        errors; -1 = no output. Defaults to 2.
  -p PLATFORM, --platform PLATFORM
                        Let the compiled code run on top of threads or
                        gameloop. The default is threads.
  -l LANGUAGE, --language LANGUAGE
                        Target language, either "csharp" or "python". Defaults
                        to the latter.
```

For a detailed explanation on the formalism's syntax please consult the latest report <a href="http://msdl.cs.mcgill.ca/people/glenn/60_Downloads">here</a>.

Tests
-------------
Executing the tests written for the Python compiler and generated Python code can be done by running `tests.py`. This file imports the test cases from the `test_files` folder.

Tanks Example
-------------
In the `tanks` folder a tanks game can be found for which both the input handling of the player-controlled tank, and the behaviour of the NPC tank are modelled using the SCCD formalism. For this specific example the commands to compile the models are as follows :

```sh
python sccdc.py tanks/player_controller.xml -o tanks/player_controller.py -p gameloop
python sccdc.py tanks/ai_controller.xml -o tanks/ai_controller.py -p gameloop
```
The resulting files `player_controller.py` and `ai_controller.py` (and any other compiled code) depend on the runtime files found in `python_runtime`, so this folder should either be put in `PYTHONPATH` or directly in the `tanks` directory (you can use a symbolic link).
