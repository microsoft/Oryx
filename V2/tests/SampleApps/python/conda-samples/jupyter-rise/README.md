# Creating interactive presentations on Binder with RISE

[![Binder](http://mybinder.org/badge.svg)](http://mybinder.org/v2/gh/binder-examples/jupyter-rise/master?filepath=index.ipynb)

RISE allows you to quickly generate a live, interactive presentation from a
Jupyter Notebook that is connected to the underlying Kernel of the notebook.
Using a new feature for automatically launching
the RISE plugin when a notebook is opened, RISE can be used to share interactive
presentations that run in the cloud with Binder.
This repository demonstrates how to accomplish this.

To make your RISE presentation automatically-launch with it is open,
add an `autolaunch=true` configuration
parameter to a notebook's `livereveal` section in the
metadata. E.g.:

```
...
"livereveal": {
        "autolaunch": true
        }
...
```

When the notebook is launched, your
presentation will automatically begin.

See the [RISE Documentation](https://damianavila.github.io/RISE/)
for more information.
