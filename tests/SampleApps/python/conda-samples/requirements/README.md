# Python environment with a requirements.txt

[![Binder](http://mybinder.org/badge_logo.svg)](http://mybinder.org/v2/gh/binder-examples/requirements/master)

A Binder-compatible repo with a `requirements.txt` file.

Access this Binder at the following URL

http://mybinder.org/v2/gh/binder-examples/requirements/master

## Notes
The `requirements.txt` file should list all Python libraries that your notebooks
depend on, and they will be installed using:

```
pip install -r requirements.txt
```

The base Binder image contains no extra dependencies, so be as
explicit as possible in defining the packages that you need. This includes
specifying explicit versions wherever possible.

If you do specify strict versions, it is important to do so for *all*
your dependencies, not just direct dependencies.
Strictly specifying only some dependencies is a recipe for environments
breaking over time.

[pip-compile](https://github.com/jazzband/pip-tools/) is a handy
tool for combining loosely specified dependencies with a fully frozen environment.
You write a requirements.in with just the dependencies you need
and pip-compile will generate a requirements.txt with all the strict packages and versions that would come from installing that package right now.
That way, you only need to specify what you actually know you need,
but you also get a snapshot of your environment.

In this example we include the library `seaborn` which will be installed in
the environment, and our notebook uses it to plot a figure.
