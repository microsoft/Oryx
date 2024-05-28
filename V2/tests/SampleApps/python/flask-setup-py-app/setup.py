#!/usr/bin/env python
import os
from setuptools import setup, find_packages

#import  testsetuppy


def read_file(filename):
    """Read a file into a string."""
    path = os.path.abspath(os.path.dirname(__file__))
    filepath = os.path.join(path, filename)
    try:
        return open(filepath).read()
    except IOError:
        return ''


setup(
    name="testsetuppy",
    author="oryx",
    author_email="oryx@oryx123.com",
    version="0.0.1",
    description="test",
    url="google",
    packages=find_packages(),
    include_package_data=True,
    install_requires=read_file("requirements/req.txt").splitlines(),
)
