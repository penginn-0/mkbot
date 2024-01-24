FROM microsoft/dotnet:2.1-sdk
WORKDIR /app

# csprojをコピーし、依存パッケージを取得
COPY *.csproj ./
RUN dotnet restore

# プログラムのコピー
COPY . ./

# リリース資材をoutフォルダに出力
RUN dotnet publish -c Release -o out

# プログラム実行
ENTRYPOINT ["dotnet", "out/App.dll"]
