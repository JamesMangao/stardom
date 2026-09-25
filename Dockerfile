# Multi-stage build for ASP.NET Web Forms (Mono on Linux)
FROM mono:6.12 AS builder

WORKDIR /src

# Copy project files and build
COPY . .

WORKDIR /src/STAR-DOM-Web/STAR-DOM-Web

# Restore / compile with msbuild
RUN nuget restore -NonInteractive || true
RUN msbuild /p:Configuration=Release /p:Platform="AnyCPU" STAR-DOM-Web.vbproj

# Runtime stage using mono and xsp4 / fastcgi-mono-server4 or apache mod_mono / xsp
FROM mono:6.12-slim

RUN apt-get update && \
    apt-get install -y mono-xsp4 mono-fastcgi-server4 curl && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /app

# Copy built site and assets
COPY --from=builder /src/STAR-DOM-Web/STAR-DOM-Web /app

# Render sets PORT environment variable (default 8095 or 10000)
ENV PORT=10000
EXPOSE 10000

# Start xsp4 web server bound to 0.0.0.0 and PORT
CMD ["sh", "-c", "xsp4 --nonstop --port=${PORT:-10000} --address=0.0.0.0 --root=/app"]
