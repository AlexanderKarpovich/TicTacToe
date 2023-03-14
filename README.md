# TicTacToe
Игра представляет собой стандартные "Крестики-нолики" для двух игроков с полем 3x3.
## API
[API](https://github.com/AlexanderKarpovich/TicTacToe/tree/dev/TicTacToe.Api) поддерживает передачу данных в формате JSON, подключение к хабу SignalR, а также передачу сообщений через gRPC.
### Начало работы
Для того чтобы приступить к работе с API, необходимо произвести настройку сертификата SSL, а также иметь представление о корректном запуске API в разных режимах (Development и Production).
##### Настройка SSL сертификата
Так как API работает с использованием HTTPS, для корректной работы сервиса необходимо произвести настройку SSL сертификата. Чтобы сделать это, необходимо открыть терминал и выполнить следующие команды:
```sh
dotnet dev-certs https -ep %APPDATA%\ASP.NET\Https\TicTacToe.Api.pfx -p pa55w0rd!
dotnet dev-certs https --trust
```
`TicTacToe.Api.pfx` отвечает за имя сертификата, а `pa55w0rd!` за пароль SSL сертификата, необходимо использовать **одинаковый пароль** во всех командах, связанных с SSL сертификатом.  
После добавления сертификата необходимо добавить пароль от сертификата в user-secrets, чтобы сделать это, в терминале **необходимо открыть папку, содержащую решение** [TicTacToe](https://github.com/AlexanderKarpovich/TicTacToe):
```sh
cd <PATH_TO_SOLUTION>\tictactoe
```
&lt;PATH_TO_SOLUTION&gt; - путь до папки с решением. Внутри папки необходимо выполнить следующую команду:
```sh
dotnet user-secrets -p TicTacToe.Api\TicTacToe.Api.csproj set "Kestrel:Certificates:Development:Password" "pa55w0rd!"
```
В данной команде в качестве пароля используется та же строка, что и при создании SSL сертификата - `"pa55w0rd!"`.
###### Смена имени и пароля SSL сертификата
Можно использовать другие имя и пароль SSL сертификата, но тогда в production файле [Docker-Compose](https://github.com/AlexanderKarpovich/TicTacToe/blob/dev/docker-compose.prod.yml) необходимо изменить путь к сертификату и стандартный пароль сертификата (**ASPNETCORE_Kestrel__Certificates__Default__Password**):
```yaml
...
services:
    tictactoeapi:
        environment:
            - ASPNETCORE_ENVIRONMENT=Production
            - ASPNETCORE_URLS=https://+:443;http://+:80
            - ASPNETCORE_Kestrel__Certificates__Default__Password=<YOUR_PASSWORD>
            - ASPNETCORE_Kestrel__Certificates__Default__Path=/root/.aspnet/https/<YOUR_CERTIFICATE_NAME>.pfx
...
```
Здесь `<YOUR_PASSWORD>` это желаемый пароль от сертификата, а `<YOUR_CERTIFICATE_NAME>` - имя сертификата.
##### Изображение API
Изображение (Image) API можно найти на [Docker Hub](https://hub.docker.com/r/alexanderkarpovich/tictactoe-api), для того чтобы его использовать в **любом** yaml файле необходимо указать изображение контейнера следующим образом:
```yaml
services:
    tictactoeapi:
        image: alexanderkarpovich/tictactoe-api
```
Изображение будет автоматически загружено с hub.docker.com, без необходимости сборки.
##### Постоянство данных MS SQL SERVER
В production сборке по умолчанию постоянство данных обеспечивается с помощью хранилищ `volumes`:
```yaml
sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    user: root
    environment:
      MSSQL_SA_PASSWORD: "pa55w0rd!"
      ACCEPT_EULA: "Y"
    volumes:
      - mssql_data:/var/opt/mssql/data:rw
      - mssql_logs:/var/opt/mssql/log:rw
      - mssql_secrets:/var/opt/mssql/secrets:rw

volumes:
  mssql_data:
  mssql_logs:
  mssql_secrets:
```
##### Запуск с использованием docker-compose
В директории [TicTacToe](https://github.com/AlexanderKarpovich/TicTacToe) находятся файлы `docker-compose.yml`, `docker-compose.override.yml` и `docker-compose.prod.yml`. Команды для запуска необходимо выполнять в директории [TicTacToe](https://github.com/AlexanderKarpovich/TicTacToe):
```sh
cd <PATH_TO_SOLUTION>/tictactoe
```
где `<PATH_TO_SOLUTION>` - путь до директории.
По умолчанию для запуска API в `Development` моде можно выполнить команду:
```sh
docker compose up
```
или
```sh
docker-compose up
```
В результате API будет работать с InMemory базами данных, что удобно для разработки, так как данные воссоздаются заново при каждом запуске.  
Для запуска API в `Production` моде необходимо выполнить следующую команду:
```sh
docker compose -f docker-compose.yml -f docker-compose.prod.yml up
```
В `Production` моде обеспечивается персистентность данных благодаря использованию хранилищ и базы данных `MS SQL SERVER`.
### Доступ к API
##### Основной доступ к API 
Основной доступ к API можно получить по следующим URL:
```sh
Games Controller:   https://localhost:5001/api/games
Users Controller:   https://localhost:5001/api/users
```
В контроллере пользователей доступны следующие actions:
```sh
Действия без авторизации:
    GET:
        Получить список игр:    https://localhost:5001/api/games
        Получить игру по ID:    https://localhost:5001/api/games/{id:int}
        Узнать победителя:      https://localhost:5001/api/games/{id:int}/winner
        
Действия, требущие авторизации:
    GET:
        Создать новую игру:     https://localhost:5001/api/games/newgame
        Присоединиться к игре:  https://localhost:5001/api/games/{id:int}/join
        Покинуть игру:          https://localhost:5001/api/games/{id:int}/leave
        Сделать шаг:            https://localhost:5001/api/games/{id:int}/makemove/{position:int}
```
Контроллер пользователей использует `ASP.NET Core Identity` в качестве фреймворка аутентификации и авторизации. В контроллере пользователей доступны следующие actions:
```sh
Действия без авторизации:
    POST:
        Войти в аккаунт:        https://localhost:5001/api/users/login
        Зарегистрироваться:     https://localhost:5001/api/users/signup
        
Действия, требущие авторизации:
    GET:
        Выйти из профиля:       https://localhost:5001/api/users/logout
```
При доступе к API в `Development` моде, можно просмотреть документацию Open API, сделанную с помощью Swagger:
```sh
https://localhost:5001/swagger/index.html
```
##### SignalR
Чтобы обеспечивать Real-Time взаимодействие пользователей с игрой, к API можно получить доступ через SignalR хаб. В JavaScript клиенте необходимо подключить скрипт к странице:
```html
<script src="https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/6.0.1/signalr.js"></script>
```
Для того, чтобы получить доступ к хабу SignalR, в клиентский код JavaScript или TypeScript необходимо вписать следующий код:
```js
const connection = new signalR.HubConnectionBuilder()
    .withUrl("https://localhost:5001/hub/games")
    .configureLogging(signalR.LogLevel.Information)
    .build();

async function start() {
    try {
        await connection.start();
        console.log("SignalR Connected.");
    } catch (err) {
        console.log(err);
        setTimeout(start, 5000);
    }
};

connection.onclose(async () => {
    await start();
});

// Start the connection.
start();
```
Для вызова метода отправки данных об игре необходимо выполнить следующий код:
```js
try {
    await connection.invoke("GameUpdated", game);
} catch (err) {
    console.error(err);
}
```
Для получения измененных данных об игре необходимо вызвать следующий код:
```js
connection.on("GameUpdated", (game) => {
    const p = document.createElement("p");
    p.textContent = `${game.gameSessionId}`;
    document.getElementById("container").appendChild(p);
});
```
##### gRPC
Для вызова gRPC процедур необходимо [создать gRPC клиент](https://learn.microsoft.com/ru-ru/aspnet/core/grpc/client?view=aspnetcore-7.0), настроив его на использование [games](https://github.com/AlexanderKarpovich/TicTacToe/blob/dev/TicTacToe.Api/Proto/games.proto) protobuf файла.
gRPC клиент может обращаться к API извне по следующей URL:
```sh
https://localhost:5001
```
Также при настройке gRPC клиента внутри сети docker compose можно обеспечить обращение к API через:
```sh
https://host.docker.internal:443
```
Доступные методы gRPC описаны в [protobuf](https://github.com/AlexanderKarpovich/TicTacToe/blob/dev/TicTacToe.Api/Proto/games.proto) файле.
