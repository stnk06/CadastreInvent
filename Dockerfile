# Этап 1: Сборка (Используем SDK 9.0)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем файл решения
COPY ["CadastreInvent.sln", "./"]

# Копируем файлы проектов из src/ и tests/
COPY src/**/*.csproj ./src/
COPY tests/**/*.csproj ./tests/

# Скрипт для распределения .csproj файлов по их папкам
RUN for file in $(ls src/*.csproj); do folder=$(basename $file .csproj); mkdir -p src/$folder/ && mv $file src/$folder/; done && \
    for file in $(ls tests/*.csproj); do folder=$(basename $file .csproj); mkdir -p tests/$folder/ && mv $file tests/$folder/; done

# Восстанавливаем все зависимости решения
RUN dotnet restore "CadastreInvent.sln"

# Копируем все остальные исходные коды
COPY . .

# Собираем основной API проект
WORKDIR "/src/src/CadastreInvent.Api"
RUN dotnet build "CadastreInvent.Api.csproj" -c Release -o /app/build

# Этап 2: Публикация
FROM build AS publish
RUN dotnet publish "CadastreInvent.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Этап 3: Финальный образ для запуска (Используем ASP.NET 9.0)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "CadastreInvent.Api.dll"]