# Docker container

The provided `Dockerfile` can be used to create a dockerized build environment where all depenencies are satisfied.

## Building the environment

In order to build a Docker container (in this example it will be namend `diplomarbeit-env`) just run the following command on a docker host (Testsd with docker 20.10.22)

```sh
docker build -t htlle-da-env .
```

This creates a container that contains all dependencies and expects to find your diploma thesis mounted into the `/workspace` folder.

Alternatively, the image can be pulled from the [Docker Hub](ttps://hub.docker.com/r/bytebang/htlle-da-env).

## Usage

One can build the thesis by running the following command:

```sh
docker run -it --rm -v $(pwd):/workspace htlle-da-env
```

This command runs the diplomarbeit-env container interactively, mounts the current directory into the container at /workspace and automatically cleans up the container after it exits.

Errors and log messages are shown in the console, the file will be written back to the `/workspace` folder.
This also works with _Docker Desktop_.

![Docker Dektop settings](img/docker-desktop.png)

**Author:** Günther Hutter
