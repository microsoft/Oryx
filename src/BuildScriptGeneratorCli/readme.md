# Oryx CLI Structure
### 1. Command Files
Option and argument inputs are handled through binders. The command files define the subcommands and register options and arguments, and export them to be added to root command in `Program.cs`.

**Developers still need to define properties used in `execute` method in command file!** Property file is intermediate step to make binder's result accessable to command file.

### 2. Binders
Each subcommand have its own custom binder. We define the expected user intput type for options and arguments and set properties.

### 3. Properties
We still keeps properties in each command class. The property file is an easy way to pass values from parsed options and arguments to `execute`.
