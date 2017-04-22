# serviceFabricDemoAzureGlobalBootcamp17
Codigo de frontend de inventario y api rest de inventario corriendo en nodos de Service Fabric.

#Presentación
https://www.slideshare.net/FernandoMeja5/azure-service-fabric-75305852

# Hands On Lab original en Ingles
Guía paso a paso para reproducir codigo desde cero:

https://github.com/Microsoft/TechnicalCommunityContent/tree/master/Cloud%20Computing/Azure%20Service%20Fabric/Session%202%20-%20Hands%20On

El codigo del hands on lab te puede dar los siguientes errores:

· Problema :El proyecto nuevo en service fabric no correra debido a referencias faltantes
· Solución: Instalar los siguientes paquetes NuGet:
    Microsoft.ServiceFabric
    Microsoft.ServiceFabric.Data
    Microsoft.ServiceFabric.Services
    Microsoft.ServiceFabric.AspNetCore.Kestrel
    Microsoft.ServiceFabric.AspNetCore.WebListener

· Problema :excepción en powershell de tipo:

Register-ServiceFabricApplicationType -ApplicationPathInImage ...
2>+         ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
2>    + CategoryInfo          : InvalidOperation: (Microsoft.Servi...usterConnection:ClusterConnection) [Register-ServiceFabricApplicationType], FabricException
2>    + FullyQualifiedErrorId : RegisterApplicationTypeErrorId,Microsoft.ServiceFabric.Powershell.RegisterApplicationType

· Solución: Crear un nuevo proyecto nuevo con un nombre diferente

· Problema :En el archivo \InventoryService\HttpCommunicationClient.cs faltan referencias e implementar interfaces
· Solución: Agregar la sentencia "using System;" como referencia

