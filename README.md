# Mikerochip's Unity Web Socket

Easy-to-use, flexible WebSocket client as a simple MonoBehaviour.

* Simple `WebSocketConnection` MonoBehaviour, just works out of the box
* Configurable with sane defaults. A URL is the only required config.
* Does not force you to use `async/await` or Coroutines
* Works with WebGL using bundled JavaScript lib `WebSocket.jslib`
* Works on other platforms using built-in `System.Net.WebSockets`
* Public API prevents corrupting the state of an active connection

# Minimum Install Requirements

Unity 2019.1 with .NET 4.x Runtime or higher

# Attribution

Based on (this repo)[https://github.com/endel/NativeWebSocket] by Endel Dreyer which was based on (this repo)[https://github.com/jirihybek/unity-websocket-webgl] by Jiri Hybek. See [license](./LICENSE.md) and [third party notices](./THIRD%20PARTY%20NOTICES.md) for full attribution.
