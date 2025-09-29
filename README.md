# Introducción 
Libreria para Request HTTP implementando configuracion de TimeOut, Retry y Circuit Breaker

# Instalacion

Se debe agregar la referencia a la libreria ```Yape.Library.Http.Client ```


# Integracion en el proyecto

La libreria http requiere de un servicio el cual debe registrarse para inyectar la dependencia 
aplicando sobre esta configuracion

### 1. Definir el servicio

Crear la clase donde se quiere inyectar la libreria http

```csharp
using Microsoft.Extensions.Logging;
using Yape.Http.Client.Infraestructure.Adapters.Http;

public class BasicApiService : IBasicApiService
{
    private readonly IResilientHttpClient _httpClient;

    public BasicApiService(HttpClient httpClient, IResilienceHttpFactory resilienceHttpFactory)
    {
        _httpClient = resilienceHttpFactory.Create(httpClient);
    }

    public async Task<MockEntity> GetDataAsync()
    {
        return await _httpClient.GetAsync<MockEntity>("/basic-api/data");
    }

    public async Task<MockEntity> Create(MockEntity data)
    {
        return await _httpClient.PostAsync<MockEntity, MockEntity>("/basic-api", data);
    }

    //resto codigo

}
```

La libreria inyecta una clase ```HttpClient``` pero se recomienda hacer uso de la extension para poder hacer
uso de funcionalidad mejorada, por eso es que se utiliza el ```CreateExtension()```

### 2. Registrar las dependencias

Utilizar el namespace 

```csharp
using Yape.Library.Http.Client.Extensions;
```

Para luego registrar el servicio

```csharp
services.AddResilientHttpClient<IBasicApiService, BasicApiService>("BasicApiClient", configuration);
```

configuration es una instancia de ```IConfiguration```

### 3. Configuracion

Existen diversas formas de configuracion dependiendo si se quiere hacer uso de llamadas:
- Http con TimeOut
- Http con Reintentos
- Http con Circuit Breaker
- Http con Reintentos y Circuit Breaker


#### 3.1 Http con TimeOut

Definir la configuracion basica implica utilizar el mismo nombre de la seccion de configuracion en ```AddResilientHttpClient```

```
{
    "HttpClients": {
        "BasicApiClient": {
            "BaseAddress": "https://localhost/",
            "DefaultHeaders": {
                "Accept": "application/json"
            },
            "DefaultTimeout": "00:00:15"
        }
    }

    .
    .
}

```

Sino se define ```DefaultTimeout``` este toma el valor de 30seg.<br>
Indicar un ```DefaultHeaders``` es opcional si se requiere

#### 3.2 Http con Reintentos

Configurar reintentos implica adicionar la propiedad ```ResiliencePolicyName```

```
{
    "HttpClients": {
        "ResilientBasicApiClient": {
	        "BaseAddress": "https://localhost/",
	        "DefaultHeaders": {
                    "Accept": "application/json"
	        },
	        "ResiliencePolicyName": "ResilientBasicApiPolicy"
        }
    },
    "ResiliencePolicies": {
        "ResilientRetryApiPolicy": {
	        "Retry": {
                    "MaxRetries": 2,
                    "Delay": "00:00:01"
	        },
	        "Timeout": {
                    "Timeout": "00:00:15"
	        }
        }
    }

    .
    .
}

```


#### 3.3 Http con Circuit Breaker

Necesitamos al menos ```MinimumThroughput``` llamadas en ```SamplingDuration```
y que el 50% (FailureRatio) de ellas fallen para que el circuito se abra.
Si ```FailureRatio``` = 0.5 y ```MinimumThroughput``` = 10, necesitamos 5 fallos en 10 llamadas.
El circuito permanecera cerrado durante ```BreakDuration```


```
{
    "HttpClients": {
        "ResilientCircuitBrekerApiClient": {
            "BaseAddress": "https://localhost/",
            "DefaultHeaders": {
                "Accept": "application/json"
            },
            "ResiliencePolicyName": "ResilientCircuitBrekerApiPolicy"
        }
    },

    "ResiliencePolicies": {
        "ResilientCircuitBrekerApiPolicy": {
            "CircuitBreaker": {
                "FailureRatio": 0.5, //50%
                "MinimumThroughput": 10,
                "SamplingDuration": "00:00:30", //30seg
                "BreakDuration": "00:00:10" //10seg
            },
            "Timeout": {
                "Timeout": "00:00:10"
            }
        }
    }
}
```

### 4. Control de errores

La libreria retorna los errores:

- HttpRequestException
- TimeoutException
- BrokenCircuitException
- Exception

Capturar estos errores se puede realizar mediante el uso del tradicional ```try..catch```, pero
tambien se puede hacer uso de un manejador de los errores.

Como base se cuenta con la interface ```IHttpErrorMapper``` la cual puede extenderse

```csharp
    public class HttpErrorMapperCustom: IHttpErrorMapper
    {
        //aqui implementacion
    }
```

Se debe registrar esta nueve interface

```csharp
 services.AddScoped<IHttpErrorMapper, HttpErrorMapperCustom>();
```

Para luego asignarla en el servicio


```csharp
public class BasicApiService : IBasicApiService
{
    private readonly IResilientHttpClient _httpClient;

    public BasicApiService(HttpClient httpClient, 
        IResilienceHttpFactory resilienceHttpFactory, 
        IHttpErrorMapper errorMapper)
    {
        var options = new HttpClientOptions()
        {
            ErrorMapper = errorMapper
        };

        _httpClient = resilienceHttpFactory.Create(httpClient, options);
    }

    public async Task<MockEntity?> GetDataAsync()
    {
        return await _httpClient.GetAsync<MockEntity>("/basic-api/data");
    }
}
```

Asignar el error custom al crear la libreria http se realiza por medio de un objeto ```HttpClientOptions```

### 5. Asignar Headers Requeridos

Por defecto la libreria va a buscar los headers

- X-Correlation-Id
- Request-Date
- Channel

y agregarlo en el request.

Los busca directamente haciendo uso de ```HttpContext```

Si no se quiere enviar los header por defecto requeridos se debe indicar por medio de options

```csharp
var options = new HttpClientOptions()
{
    IncludeHeadersRequired = false
};
```    

En el servicio se define al crear la instancia

```csharp
public class BasicApiService : IBasicApiService
{
    private readonly IResilientHttpClient _httpClient;

    public BasicApiService(HttpClient httpClient, 
        IResilienceHttpFactory resilienceHttpFactory, 
        IHttpErrorMapper errorMapper)
    {
        var options = new HttpClientOptions()
        {
            IncludeHeadersRequired = false
        };

        _httpClient = resilienceHttpFactory.Create(httpClient, options);
    }

    public async Task<MockEntity?> GetDataAsync()
    {
        return await _httpClient.GetAsync<MockEntity>("/basic-api/data");
    }
}
```