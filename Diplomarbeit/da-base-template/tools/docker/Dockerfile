FROM ubuntu:18.04

# Set the environment variables
ENV LANG=C.UTF-8 \
    LC_ALL=C.UTF-8 \
    DEBIAN_FRONTEND=noninteractive

# Update the package list and install dependencies
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
    curl \
    tree \
    rsync \
    git \
    make \
    build-essential \
    texlive-full \
    pandoc \
    pandoc-citeproc \
    hunspell \
    hunspell-de-at \
    hunspell-en-us && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Set the working directory
WORKDIR /workspace

# Set the entrypoint to the script
ENTRYPOINT ["make", "pdf", "-C", "da-base-template", "SOURCEDIR=/workspace"]
