# 使用多階段構建來分離建構和運行階段

# 建構階段
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# 定義目標架構變數
ARG TARGETARCH
# 設定工作目錄
WORKDIR /source

# 複製專案檔並還原相依性
COPY *.csproj ./
RUN dotnet restore -r linux-$TARGETARCH

# 複製其餘原始碼並發布應用程式
COPY . ./
RUN dotnet publish -c Release -r linux-$TARGETARCH --no-restore -o /app/publish

# 運行階段
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
# 開放應用程式埠
EXPOSE 8080
# 設定工作目錄
WORKDIR /app
# 複製發布好的應用程式
COPY --from=build /app/publish .
# 設定容器啟動時執行的命令
ENTRYPOINT ["dotnet", "aspnetcoreapp.dll"]

