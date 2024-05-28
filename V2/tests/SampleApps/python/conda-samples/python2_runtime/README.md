# Specifying a Python 2 environment with `runtime.txt`

[![Binder](http://mybinder.org/badge_logo.svg)](http://beta.mybinder.org/v2/gh/binder-examples/python2_runtime/master)

We can specify various runtime parameters with a `runtime.txt` file. In this
repository, we demonstrate how to install python 2 with the environment.

If you specify `python-2.7` in `runtime.txt`, then:

* A python3 environment is created & installed (this is what the notebook runs from)
* A python2 environment is created and registered
* The contents of `requirements.txt` are installed into the python2 environment

**important:**
Make sure that you save your notebooks with a python 2 kernel activated,
as this defines which kernel Binder will use when a notebook is opened.

**note:**
If you *also* wish to install python 3 dependencies, you may do so
by including a file called `requirements3.txt`. The packages
inside will be installed into the python 3 environment.
