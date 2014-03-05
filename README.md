Statecharts and Class Diagram Compiler
======================================

Usage
-------------
```sh
$python sccdc.py --help
usage: sccdc.py [-h] [-o OUTPUT] [-v VERBOSE] [-p PROTOCOL] [-l LANGUAGE]
                input

positional arguments:
  input                 The path to the XML file to be compiled.

optional arguments:
  -h, --help            show this help message and exit
  -o OUTPUT, --output OUTPUT
                        The path to the target python file. Defaults to the
                        same name as the input file.
  -v VERBOSE, --verbose VERBOSE
                        0 = no output, 1 = only show warnings, 2 = show all
                        output. Defaults to 2.
  -p PROTOCOL, --protocol PROTOCOL
                        Let the compiled code run on top of threads or
                        gameloop. The default is threads.
  -l LANGUAGE, --language LANGUAGE
                        Target language, either "csharp" or "python". Defaults
                        to the latter.
```

For a detailed explanation on the formalism's syntax please consult the latest report <a href="http://msdl.cs.mcgill.ca/people/glenn/60_Downloads">here</a>.

Tests
-------------
Executing the unit tests can be done by running `tests.py`. This file imports the test cases from the `test_files` folder.

Tanks Example
-------------
In the `tanks` folder a tanks game can be found for which both the input handling of the player-controlled tank, and the behaviour of the NPC tank are modelled using the SCCD formalism. For this specific example the commands to compile the models are as follows :

```sh
python SCCDC.py tanks/player_controller.xml -o tanks/player_controller.py -p gameloop
python SCCDC.py tanks/ai_controller.xml -o tanks/ai_controller.py -p gameloop
```
The resulting files `player_controller.py` and `player_controller.py` (and any other compiled code) depend on the runtime files found in `python_runtime`. 