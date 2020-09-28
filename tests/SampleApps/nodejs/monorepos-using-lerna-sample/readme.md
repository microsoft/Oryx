<p align="center">
  <img src="https://user-images.githubusercontent.com/6391763/82353068-8c30ed80-9a1c-11ea-85ff-492ea3feb709.png" alt="Logo"/>
</p>

<p align="center">
  <a href="https://twitter.com/nirmalyaghosh23">
    <img alt="Twitter: Nirmalya Ghosh" src="https://img.shields.io/twitter/follow/nirmalyaghosh23.svg?style=social" target="_blank" />
  </a>
</p>

This is a boilerplate for building mono-repo applications using [Lerna](https://lerna.js.org/). This mon-orepo consists of the following packages:

1. [**front-end**](https://github.com/ghoshnirmalya/building-monorepos-using-lerna/tree/master/packages/front-end): Next.js application which uses the [Button component from the component package](https://github.com/ghoshnirmalya/building-monorepos-using-lerna/blob/master/packages/front-end/pages/index.js#L2)
4. [**components**](https://github.com/ghoshnirmalya/building-monorepos-using-lerna/tree/master/packages/components): Sample React.js application with Storybook for creating a Design System

<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->
**Table of Contents**

- [Overview](#overview)
- [Demo](#demo)
- [Requirements](#requirements)
- [Installation](#installation)
  - [1. **Clone the application**](#1-clone-the-application)
  - [2. **Install Lerna globally**](#2-install-lerna-globally)
  - [3. **Bootstrap the packages**](#3-bootstrap-the-packages)
  - [4. **Start the packages**](#4-start-the-packages)
- [License](#license)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

## Overview

This boilerplate is built using [Lerna](https://lerna.js.org/) for managing all the packages in a simple manner. Because of Lerna, it becomes very easy to install, develop and maintain a mono-repo structure.

## Demo

A demo of this application is hosted [here](https://lerna-monorepo.now.sh/).

## Requirements

1. [Node.js](https://nodejs.org/)
2. [npm](https://www.npmjs.com/)
3. [Lerna](https://lerna.js.org/)

## Installation

### 1. **Clone the application**

```sh
git clone https://github.com/ghoshnirmalya/building-monorepos-using-lerna
```

### 2. **Install Lerna globally**

```sh
npm install --global lerna
```

### 3. **Bootstrap the packages**

From the project root, we can run the following command to bootstrap the packages and install all their dependencies and linking any cross-dependencies:

```sh
lerna bootstrap
```

### 4. **Start the packages**

From the project root, we can run the following command to start our Node.js packages:

```sh
lerna run dev --parallel
```

The above command will do the following:

    a. Start the front-end package on [http://localhost:3000/](http://localhost:3000).
    b. Start watching changes for the components package

## License

This project is licensed under the [MIT License](https://opensource.org/licenses/MIT).
