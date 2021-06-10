FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build-env

WORKDIR /app

# sln and project files
#COPY *.sln ./
COPY ./lib/wolfereiter-graph-claimstransform/src/WolfeReiter.Identity.Claims/WolfeReiter.Identity.Claims.csproj ./lib/wolfereiter-graph-claimstransform/src/WolfeReiter.Identity.Claims/
COPY ./src/WolfeReiter.Identity.Data/WolfeReiter.Identity.Data.csproj ./src/WolfeReiter.Identity.Data/
COPY ./src/WolfeReiter.Identity.DualStack/WolfeReiter.Identity.DualStack.csproj ./src/WolfeReiter.Identity.DualStack/

# resstore packages
RUN dotnet restore lib/wolfereiter-graph-claimstransform/src/WolfeReiter.Identity.Claims
RUN dotnet restore src/WolfeReiter.Identity.Data
RUN dotnet restore src/WolfeReiter.Identity.DualStack

# copy everything else
COPY . ./
# build release artifacts
RUN dotnet publish src/WolfeReiter.Identity.DualStack -c Release -o out --no-self-contained
RUN rm out/appsettings.Development.json

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine

WORKDIR /app
COPY --from=build-env /app/out .

# disable global invariant for EF Core:
# Application startup exception: System.NotSupportedException: Globalization Invariant Mode is not supported.
# https://github.com/dotnet/efcore/issues/18025
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

ENTRYPOINT [ "dotnet", "WolfeReiter.Identity.DualStack.dll" ]