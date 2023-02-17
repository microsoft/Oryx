# Oryx Cli Structure
## Background
Due to the transition to .NET 7, we found that the past CLI package isn't compatible with the new framework. Hence, we've decided to move to `System.CommandLine` package to develop our CLI. For developers, the structure of the CLI codes are different then the past.

## Structure
- Program.cs (Define root command `oryx` and add subcommands)
  - Subcommand.cs (Define subcommands, register its options and arguments, and define the execute action)

### 1. Command Files
The command folder will not handle option and argument inputs directly. This will be done in the binder. The command file will only define the subcommands, register its options and arguments, and define the `Execute` action. The command (with all its options and executes) will then be exported to be added to the root command.

**Developer will still need to define the properties used in the `execute` method in command file!** The property file is a intermediate step to make the binder's result accessable to the command file.

### 2. Binders
With the transition, we've updated command folder with binder folder. Each subcommand will have its own custom binder defined. This is where `System.CommandLine` package actually handles user inputs in each option. We define the expected user intput type for options and arguments and set property.

### 3. Properties
Oryx now has a new folder in the CLI project that contains all the properties the CLI commmands need to execute. We still keeps different properties getter and setter in each command class since most of the commands `execute` method is using them. The property file is a easy way to pass the values from parsed options and arguments to the execute.
