# Portfolio.Infrastructure

Capa de infraestructura de persistencia y servicios técnicos. Contiene EF Core, `PortfolioDbContext`, configuraciones, migraciones, consultas y servicios de administración, además de Identity y almacenamiento de medios fuera de SQL Server y `wwwroot`.

Los medios de certificación se configuran mediante `Portfolio:CertificationMedia` (directorio predeterminado `App_Data/CertificationMedia`, límite de 10 MiB por archivo y formatos PDF/JPG/JPEG/PNG). Los nombres visibles de adjuntos son editables sin alterar el nombre original ni la clave de almacenamiento. `20260930222211_AddCertificationMetadataAndAttachmentDisplayName` añade ID/horas opcionales y el nombre visible, copiando el nombre original existente como valor inicial. La migración está generada y no aplicada; debe aplicarse explícitamente en cada base de datos de destino.
