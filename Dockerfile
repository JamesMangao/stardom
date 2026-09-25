# Use Ubuntu 22.04 LTS with current Mono repository & XSP4
FROM ubuntu:22.04

ENV DEBIAN_FRONTEND=noninteractive
ENV TZ=UTC

# Install prerequisites, Mono official repo, MSBuild, and Mono-XSP4
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
        ca-certificates \
        gnupg \
        dirmngr \
        curl && \
    gpg --homedir /tmp --no-default-keyring --keyring /usr/share/keyrings/mono-official-archive-keyring.gpg --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF && \
    echo "deb [signed-by=/usr/share/keyrings/mono-official-archive-keyring.gpg] https://download.mono-project.com/repo/ubuntu stable-focal main" | tee /etc/apt/sources.list.d/mono-official-stable.list && \
    apt-get update && \
    apt-get install -y --no-install-recommends \
        mono-devel \
        mono-xsp4 \
        nuget \
        msbuild && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /app

# Copy project files
COPY . /app

WORKDIR /app/STAR-DOM-Web/STAR-DOM-Web

# Build the VB.NET Web application
RUN msbuild /p:Configuration=Release /p:Platform="AnyCPU" STAR-DOM-Web.vbproj || true

WORKDIR /app/STAR-DOM-Web/STAR-DOM-Web

# Render dynamic port binding (defaults to 10000)
ENV PORT=10000
EXPOSE 10000

# Start Mono XSP4 web server
CMD ["sh", "-c", "xsp4 --nonstop --port=${PORT:-10000} --address=0.0.0.0 --root=/app/STAR-DOM-Web/STAR-DOM-Web"]
