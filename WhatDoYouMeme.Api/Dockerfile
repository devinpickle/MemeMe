FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["WhatDoYouMeme.Api/WhatDoYouMeme.Api.csproj", "WhatDoYouMeme.Api/"]
RUN dotnet restore "WhatDoYouMeme.Api/WhatDoYouMeme.Api.csproj"

COPY . .
WORKDIR "/src/WhatDoYouMeme.Api"

RUN dotnet publish "WhatDoYouMeme.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends libfontconfig1 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://0.0.0.0:${PORT:-10000} exec dotnet WhatDoYouMeme.Api.dll"]