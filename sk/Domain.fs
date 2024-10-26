module sk.Domain

type SKConfig =
    { Endpoint: string
      Key: string
      ModelName: string
      ModelVersion: string }

type Deployments =
    | Local
    | Azure
