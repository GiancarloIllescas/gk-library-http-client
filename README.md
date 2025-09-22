# Introducción 
Libreria para Request HTTP implementando configuracion de TimeOut, Retry y Circuit Breaker

# Instalacion


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
using Yape.Http.Client.DependencyInjection;
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
tambien se puede hacer uso de un manejador de los errores que anularia propagar el error.

Como base se cuenta con una clase ```ErrorMapperBase``` la cual puede extenderse

```csharp
    public class ErrorMapperMock : ErrorMapperBase
    {
        public ErrorMapperMock(ILoggerFactory loggerFactory) : base(loggerFactory)
        {
        }

        public override void HttpRequestFailed(HttpRequestException ex)
        {
            // codigo ante un error
        }
    }
```

Para luego asignarla en el servicio


```csharp
    public class BasicApiService : IBasicApiService
    {
        private readonly ILogger<BasicApiService> _logger;
        private readonly IResilientHttpClient _httpClient;

        public BasicApiService(HttpClient httpClient,       IResilienceHttpFactory resilienceHttpFactory, ILogger<BasicApiService> logger)
        {
            _logger_ = logger;
            _httpClient = resilienceHttpFactory.Create(httpClient);
        }

        public async Task<MockEntity?> Update(MockEntity data)
        {
            _httpClient.ErrorMapper = new ErrorMapperMock(_logger);

            return await _httpClient.PutAsync<MockEntity, MockEntity>("/basic-api", data);
        }

        .
        .

    }
```

