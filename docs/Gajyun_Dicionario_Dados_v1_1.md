# Dicionário de dados: Guesthouse Gajyun

Fonte: `Guesthouse_Gajyun_Schema.sql`, versão 1.0. Este documento descreve **os campos existentes**, não altera a base de dados.

**Notação:** PK = chave primária; FK = chave estrangeira; NULL = valor opcional; BIT = booleano; DATETIME2 = data e hora.

## Tabelas existentes (21)

### OTA
Plataformas de reserva.

| Campo | Tipo SQL | Opcional? | Significado |
|---|---|---|---|
| `OTAID` | `INT` | Não | Identificador da plataforma de reservas |
| `OTAName` | `NVARCHAR(120)` | Não | Nome comercial da plataforma de reservas |
| `LogoUrl` | `NVARCHAR(500)` | Sim | URL opcional do logótipo |
| `IsActive` | `BIT` | Não | Indica se o registo está ativo |

### BookingStatus
Estados possíveis das reservas.

| Campo | Tipo SQL | Opcional? | Significado |
|---|---|---|---|
| `BookingStatusID` | `INT` | Não | Identificador do estado da reserva |
| `StatusName` | `NVARCHAR(60)` | Não | Nome do estado de reserva |
| `IsActive` | `BIT` | Não | Indica se o registo está ativo |

### Country
Países e nacionalidades.

| Campo | Tipo SQL | Opcional? | Significado |
|---|---|---|---|
| `CountryID` | `INT` | Não | Identificador do país |
| `CountryName` | `NVARCHAR(120)` | Não | Nome do país |
| `ISO2Code` | `CHAR(2)` | Sim | Código ISO de duas letras do país |
| `FlagUrl` | `NVARCHAR(500)` | Sim | URL de imagem da bandeira |

### Room
Os quartos físicos internos da guesthouse.

| Campo | Tipo SQL | Opcional? | Significado |
|---|---|---|---|
| `RoomID` | `INT` | Não | Identificador do quarto físico interno |
| `RoomName` | `NVARCHAR(100)` | Não | Nome interno do quarto físico |
| `IsActive` | `BIT` | Não | Indica se o registo está ativo |

### Bed
Camas físicas, cada uma pertencente a um quarto.

| Campo | Tipo SQL | Opcional? | Significado |
|---|---|---|---|
| `BedID` | `INT` | Não | Identificador da cama física |
| `RoomID` | `INT` | Não | Identificador do quarto físico interno |
| `BedLabel` | `NVARCHAR(80)` | Não | Identificação da cama dentro do quarto |
| `IsActive` | `BIT` | Não | Indica se o registo está ativo |

### PaymentMethod
Métodos de pagamento.

| Campo | Tipo SQL | Opcional? | Significado |
|---|---|---|---|
| `PaymentMethodID` | `INT` | Não | Identificador do método de pagamento |
| `MethodName` | `NVARCHAR(80)` | Não | Nome do método de pagamento |
| `IsActive` | `BIT` | Não | Indica se o registo está ativo |

### DocumentType
Categorias de documentos.

| Campo | Tipo SQL | Opcional? | Significado |
|---|---|---|---|
| `DocumentTypeID` | `INT` | Não | Identificador do tipo de documento |
| `DocumentTypeName` | `NVARCHAR(100)` | Não | Designação do tipo de documento |
| `IsActive` | `BIT` | Não | Indica se o registo está ativo |

### ExtraService
Catálogo de serviços adicionais.

| Campo | Tipo SQL | Opcional? | Significado |
|---|---|---|---|
| `ExtraServiceID` | `INT` | Não | Identificador do serviço extra |
| `ServiceName` | `NVARCHAR(100)` | Não | Nome do serviço extra |
| `DefaultUnitPrice` | `DECIMAL(12,2)` | Não | Preço sugerido do serviço; não altera vendas antigas |
| `CurrencyCode` | `CHAR(3)` | Não | Código da moeda com três letras (por exemplo JPY) |
| `IsActive` | `BIT` | Não | Indica se o registo está ativo |

### ReservationGroup
Grupo de reservas relacionadas.

| Campo | Tipo SQL | Opcional? | Significado |
|---|---|---|---|
| `GroupID` | `BIGINT` | Não | Identificador do grupo de reservas |
| `ExternalGroupReference` | `NVARCHAR(120)` | Sim | Identificador externo opcional do grupo |
| `Notes` | `NVARCHAR(1000)` | Sim | Notas livres associadas ao registo |

### Customer
Pessoas/hóspedes conhecidos.

| Campo | Tipo SQL | Opcional? | Significado |
|---|---|---|---|
| `CustomerID` | `BIGINT` | Não | Identificador interno do hóspede |
| `FullName` | `NVARCHAR(255)` | Não | Nome completo do hóspede |
| `Nickname` | `NVARCHAR(255)` | Sim | Nome pelo qual o hóspede prefere ser tratado |
| `CountryID` | `INT` | Sim | Identificador do país |
| `PostalAddress` | `NVARCHAR(500)` | Sim | Morada postal do hóspede |
| `PhoneNumber` | `NVARCHAR(50)` | Sim | Contacto telefónico |
| `Email` | `NVARCHAR(254)` | Sim | Email do hóspede |
| `IsRegularCustomer` | `BIT` | Não | Indica hóspede frequente |
| `CreatedAt` | `DATETIME2(0)` | Não | Data/hora de criação local do registo |

### Reservation
Registo original e operacional de uma reserva.

| Campo | Tipo SQL | Opcional? | Significado |
|---|---|---|---|
| `ReservationID` | `BIGINT` | Não | Identificador interno da reserva |
| `OTAID` | `INT` | Não | Identificador da plataforma de reservas |
| `ExternalReservationNumber` | `NVARCHAR(120)` | Sim | Identificador de reserva atribuído pela OTA; único dentro da OTA quando preenchido |
| `GroupID` | `BIGINT` | Sim | Identificador do grupo de reservas |
| `BookingStatusID` | `INT` | Não | Identificador do estado da reserva |
| `BookedAt` | `DATETIME2(0)` | Sim | Data/hora em que a reserva foi feita na origem |
| `ModifiedAtSource` | `DATETIME2(0)` | Sim | Última modificação conhecida na OTA |
| `CheckInDate` | `DATE` | Não | Primeiro dia da estadia (chegada prevista) |
| `CheckOutDate` | `DATE` | Não | Dia de saída previsto; não ocupa essa noite |
| `ExpectedGuestCount` | `SMALLINT` | Sim | Quantidade esperada de hóspedes, mesmo se nem todos estiverem identificados |
| `ActualCheckInAt` | `DATETIME2(0)` | Sim | Data e hora efetivas de check-in introduzidas pela equipa |
| `CheckInStatus` | `NVARCHAR(20)` | Não | Pending, CheckedIn ou NoShow |
| `IsActive` | `BIT` | Não | Indica se o registo está ativo |
| `IsModified` | `BIT` | Não | Indicador de que a reserva foi alterada |
| `ConsecutiveStayInfo` | `NVARCHAR(MAX)` | Sim | Texto importado sobre possíveis estadias consecutivas; não estabelece ligação |
| `RoomRateInfo` | `NVARCHAR(MAX)` | Sim | Detalhes textuais da tarifa; não deve ser somado como cobrança |
| `BookingDetails` | `NVARCHAR(MAX)` | Sim | Detalhes gerais importados da reserva |
| `CreatedAt` | `DATETIME2(0)` | Não | Data/hora de criação local do registo |
| `UpdatedAt` | `DATETIME2(0)` | Não | Data de atualização local; aplicação deve atualizá-la |

### ReservationCustomer
Associação muitos-para-muitos entre hóspedes e reservas.

| Campo | Tipo SQL | Opcional? | Significado |
|---|---|---|---|
| `ReservationID` | `BIGINT` | Não | Identificador interno da reserva |
| `CustomerID` | `BIGINT` | Não | Identificador interno do hóspede |
| `IsLeadGuest` | `BIT` | Não | Marca o hóspede principal; no máximo um por reserva |
| `AddedAt` | `DATETIME2(0)` | Não | Data da associação do cliente à reserva |

### ReservationAccommodation
Atribuição temporal de quarto ou cama.

| Campo | Tipo SQL | Opcional? | Significado |
|---|---|---|---|
| `ReservationAccommodationID` | `BIGINT` | Não | Identificador único da atribuição de alojamento |
| `ReservationID` | `BIGINT` | Não | Identificador interno da reserva |
| `RoomID` | `INT` | Não | Identificador do quarto físico interno |
| `BedID` | `INT` | Sim | Identificador da cama física |
| `StartDate` | `DATE` | Não | Primeira noite deste alojamento/atribuição |
| `EndDate` | `DATE` | Não | Dia final, exclusivo; não ocupa esta noite |
| `Notes` | `NVARCHAR(300)` | Sim | Notas livres associadas ao registo |

### ReservationCharge
Itens cobrados, incluindo diárias e extras.

| Campo | Tipo SQL | Opcional? | Significado |
|---|---|---|---|
| `ChargeID` | `BIGINT` | Não | Identificador da cobrança |
| `ReservationID` | `BIGINT` | Não | Identificador interno da reserva |
| `ExtraServiceID` | `INT` | Sim | Identificador do serviço extra |
| `ChargeType` | `NVARCHAR(30)` | Não | Categoria: Accommodation, Breakfast, Laundry, Towel, LateCheckIn, Tax, Discount, Adjustment ou Other |
| `ServiceDate` | `DATE` | Sim | Noite/data a que se refere a cobrança |
| `Description` | `NVARCHAR(500)` | Sim | Descrição textual da cobrança ou do plano |
| `Quantity` | `DECIMAL(10,2)` | Não | Quantidade facturada; para diária total do quarto geralmente 1 |
| `UnitPrice` | `DECIMAL(12,2)` | Não | Preço unitário; negativo só em desconto/ajuste |
| `Amount` | `AS` | Calculado | Montante; na cobrança é calculado como Quantity × UnitPrice |
| `CurrencyCode` | `CHAR(3)` | Não | Código da moeda com três letras (por exemplo JPY) |
| `SourceLineKey` | `NVARCHAR(150)` | Sim | Chave estável da linha importada para evitar duplicação |
| `CreatedAt` | `DATETIME2(0)` | Não | Data/hora de criação local do registo |

### ReservationPayment
Movimentos reais de dinheiro.

| Campo | Tipo SQL | Opcional? | Significado |
|---|---|---|---|
| `PaymentID` | `BIGINT` | Não | Identificador do pagamento |
| `ReservationID` | `BIGINT` | Não | Identificador interno da reserva |
| `PaymentMethodID` | `INT` | Sim | Identificador do método de pagamento |
| `Direction` | `NVARCHAR(10)` | Não | Inflow ou Outflow, da perspetiva da guesthouse |
| `CounterpartyType` | `NVARCHAR(20)` | Não | Guest, OTA, Guesthouse ou Other |
| `PaymentKind` | `NVARCHAR(25)` | Não | Payment, Refund, Payout ou Other |
| `Amount` | `DECIMAL(12,2)` | Não | Montante positivo do movimento real; valor armazenado, não calculado a partir das cobranças |
| `CurrencyCode` | `CHAR(3)` | Não | Código da moeda com três letras (por exemplo JPY) |
| `PaidAt` | `DATETIME2(0)` | Sim | Data/hora do movimento financeiro efetivo |
| `ExternalReference` | `NVARCHAR(150)` | Sim | Referência externa do pagamento |
| `Notes` | `NVARCHAR(500)` | Sim | Notas livres associadas ao registo |

### OTAStatementItem
Valores declarados pela OTA, mesmo que ainda não classificados.

| Campo | Tipo SQL | Opcional? | Significado |
|---|---|---|---|
| `OTAStatementItemID` | `BIGINT` | Não | Identificador único do item de extrato OTA |
| `ReservationID` | `BIGINT` | Não | Identificador interno da reserva |
| `RawLabel` | `NVARCHAR(100)` | Não | Rótulo literal recebido da OTA, por exemplo deposit |
| `RawText` | `NVARCHAR(500)` | Sim | Texto de origem referente ao item |
| `Amount` | `DECIMAL(12,2)` | Não | Montante declarado pela OTA; valor armazenado, não comprova recebimento ou comissão |
| `CurrencyCode` | `CHAR(3)` | Não | Código da moeda com três letras (por exemplo JPY) |
| `Classification` | `NVARCHAR(25)` | Não | Unclassified, Commission, GuestDeposit, Payout, TaxWithheld ou Other |
| `ResolutionNotes` | `NVARCHAR(500)` | Sim | Notas de reconciliação do valor OTA |
| `ImportedAt` | `DATETIME2(0)` | Não | Data/hora de importação |
| `SourceLineKey` | `NVARCHAR(150)` | Sim | Chave estável da linha importada para evitar duplicação |

### PaymentAllocation
Atribuição de montantes pagos a cobranças.

| Campo | Tipo SQL | Opcional? | Significado |
|---|---|---|---|
| `PaymentID` | `BIGINT` | Não | Identificador do pagamento |
| `ChargeID` | `BIGINT` | Não | Identificador da cobrança |
| `AllocatedAmount` | `DECIMAL(12,2)` | Não | Parcela de pagamento afetada a uma cobrança |

### ReservationMemo
Notas associadas à reserva.

| Campo | Tipo SQL | Opcional? | Significado |
|---|---|---|---|
| `MemoID` | `BIGINT` | Não | Identificador da nota |
| `ReservationID` | `BIGINT` | Não | Identificador interno da reserva |
| `MemoText` | `NVARCHAR(MAX)` | Não | Texto da anotação interna |
| `CreatedAt` | `DATETIME2(0)` | Não | Data/hora de criação local do registo |

### CustomerDocument
Documentação opcional dos clientes.

| Campo | Tipo SQL | Opcional? | Significado |
|---|---|---|---|
| `DocumentID` | `BIGINT` | Não | Identificador do documento |
| `CustomerID` | `BIGINT` | Não | Identificador interno do hóspede |
| `DocumentTypeID` | `INT` | Não | Identificador do tipo de documento |
| `FileStorageKey` | `NVARCHAR(1000)` | Sim | Caminho/chave local opcional do ficheiro |
| `PhotoStorageKey` | `NVARCHAR(1000)` | Sim | Caminho/chave local opcional da fotografia |
| `ExpiryDate` | `DATE` | Sim | Data de expiração do documento, se existir |
| `CreatedAt` | `DATETIME2(0)` | Não | Data/hora de criação local do registo |

### ImportBatch
Cada execução/fichiro de importação.

| Campo | Tipo SQL | Opcional? | Significado |
|---|---|---|---|
| `ImportBatchID` | `BIGINT` | Não | Identificador do lote de importação |
| `OTAID` | `INT` | Não | Identificador da plataforma de reservas |
| `SourceFileName` | `NVARCHAR(255)` | Sim | Nome do ficheiro CSV de origem |
| `ImportedAt` | `DATETIME2(0)` | Não | Data/hora de importação |

### ImportRow
Cada linha original lida de um ficheiro.

| Campo | Tipo SQL | Opcional? | Significado |
|---|---|---|---|
| `ImportRowID` | `BIGINT` | Não | Identificador de uma linha importada |
| `ImportBatchID` | `BIGINT` | Não | Identificador do lote de importação |
| `SourceRowNumber` | `INT` | Não | Número da linha no CSV de origem |
| `ReservationID` | `BIGINT` | Sim | Identificador interno da reserva |
| `RawDetails` | `NVARCHAR(MAX)` | Sim | Conteúdo original do campo 詳細, quando disponível |
| `RawPayload` | `NVARCHAR(MAX)` | Não | Registo original integral importado, preservado para auditoria |
| `ParseStatus` | `NVARCHAR(20)` | Não | Pending, Processed, Failed ou Skipped |
| `ParseError` | `NVARCHAR(2000)` | Sim | Descrição do erro de parsing/validação |

## View financeira

`vwReservationFinancialSummary` agrega valores por reserva e moeda.

| Campo | Significado |
|---|---|
| `ReservationID` | Reserva consultada |
| `CurrencyCode` | Moeda |
| `TotalCharged` | Soma dos itens em ReservationCharge |
| `NetCashMovement` | Recebimentos menos saídas em ReservationPayment |
| `UnreconciledDifference` | Diferença entre cobranças e movimentos financeiros; não é automaticamente dívida real |

Os dados OTA ainda não classificados são excluídos desta view.

## Alterações recomendadas para a versão 1.1 (ainda NÃO presentes no SQL)

| Entidade nova / alteração | Finalidade |
|---|---|
| `Property` (`PropertyID`, `PropertyName`, `MaxOccupancy`, `IsActive`) | Capacidade total configurável de 18 hóspedes |
| `Room.Capacity`, se aplicável | Capacidade máxima física do quarto |
| `TaxPolicy` (`TaxPolicyID`, `PolicyName`, `EffectiveFrom`, `EffectiveTo`, `IsActive`) | Versões da regra fiscal em função da data da noite |
| `TaxBracket` (`TaxBracketID`, `TaxPolicyID`, `MinPriceInclusive`, `MaxPriceExclusive`, `TaxPerPersonNight`) | Escalões fiscais por pessoa/noite, sem sobreposições |
| `ReservationTax` (`ReservationTaxID`, `ReservationID`, `ServiceDate`, `TaxPolicyID`, `GuestCount`, `TaxableRoomPricePerPerson`, `LegalTaxAmount`, `CollectedTaxAmount`, `Status`) | Histórico de apuramento fiscal, distinto do cobrado |
| `Stay` (`StayID`, `StartDate`, `EndDate`, `Notes`) | Estadia operacional que pode ter várias reservas |
| `StayReservation` (`StayID`, `ReservationID`) | Associação de reservas consecutivas sem fundir registos OTA |
| `ReservationGroup.GroupName`, `GroupType`, `CreatedAt` | Identificar grupos descobertos após o booking |
| `OTARoomMapping` (`OTARoomMappingID`, `OTAID`, `ExternalRoomCode`, `ExternalRoomName`, `RoomID`, `IsActive`) | Traduzir nomes comerciais em quartos internos |
| `OTARoomType`, se necessário | Categorias comercializadas que podem representar vários quartos/camas |

## Regras que exigem código/transações

1. Verificar lotação por **cada noite** no intervalo `[CheckInDate, CheckOutDate)` e não contar reservas canceladas/no-show segundo a política definida.
2. Verificar conflitos de camas/quartos com transações e bloqueios adequados.
3. Evitar cobrança duplicada de taxa: `ReservationTax` é o cálculo; `ReservationCharge` contém o débito financeiro quando realmente aplicado.
4. Aplicar regra fiscal pela data da noite, preço elegível por pessoa/noite e número de pessoas.
5. Conservar reservas originais separadas ao associá-las a uma `Stay` ou `ReservationGroup`.
6. Mapeamentos OTA desconhecidos devem gerar revisão manual e não novos quartos físicos.
7. Rever regras legais da cidade de Kyoto antes de usar valores fiscais em produção.


---

# Anexo B: especificação proposta v1.1 (a implementar)

**Atenção:** as 21 tabelas descritas acima existem apenas no script v1.0. As seis tabelas abaixo e as alterações a `Room` são **propostas para a migração inicial do novo projeto**, e não fazem parte do SQL original. O Copilot deve confrontar cada nome e tipo com o ficheiro `.sql` antes de gerar entidades.

## Alterações propostas a `Room`

| Campo | Tipo SQL proposto | Nulo | Significado |
|---|---|---|---|
| `Capacity` | `SMALLINT` | Não | Capacidade máxima de hóspedes no quarto. Não representa número de unidades vendidas. CHECK > 0. |
| `DefaultTaxPolicyID` | `INT` | Sim | FK para `TaxPolicy`: política interna selecionada na configuração do quarto. **Não altera a legislação aplicável.** |

Seed inicial de quartos: `順 (Jyun)` = 4; `翔 (Show)` = 2; `智 (Tomo)` = 4; `杏 (Anne)` = 2; `雅 (Miyabi)` = 6. Total físico = 18. Os IDs são gerados pelo SQL, não codificados fixamente.

## `TaxPolicy` (nova)

| Campo | Tipo SQL proposto | Nulo | Significado |
|---|---|---|---|
| `TaxPolicyID` | `INT IDENTITY` PK | Não | Identificador da regra fiscal. |
| `PolicyName` | `NVARCHAR(160)` UNIQUE | Não | Nome apresentado no Settings, ex. Kyoto City Accommodation Tax 2026. |
| `ValidFrom` | `DATE` | Não | Início de vigência legal informada. |
| `ValidTo` | `DATE` | Sim | Fim de vigência legal; NULL se não definido. |
| `IsActive` | `BIT` | Não | Se a regra pode ser mostrada/selecionada no catálogo. |
| `Notes` | `NVARCHAR(1000)` | Sim | Referência da fonte e observações. |

## `TaxBracket` (nova)

| Campo | Tipo SQL proposto | Nulo | Significado |
|---|---|---|---|
| `TaxBracketID` | `INT IDENTITY` PK | Não | Identificador do escalão. |
| `TaxPolicyID` | `INT` FK | Não | Política à qual o escalão pertence. |
| `MinPriceInclusive` | `DECIMAL(12,2)` | Não | Limite mínimo de preço tributável por pessoa/noite. |
| `MaxPriceExclusive` | `DECIMAL(12,2)` | Sim | Limite máximo exclusivo; NULL significa sem limite superior. |
| `TaxPerPersonNight` | `DECIMAL(12,2)` | Não | Imposto por hóspede/noite neste escalão. |

## `ReservationTax` (nova)

| Campo | Tipo SQL proposto | Nulo | Significado |
|---|---|---|---|
| `ReservationTaxID` | `BIGINT IDENTITY` PK | Não | Identificador do cálculo/snapshot. |
| `ReservationID` | `BIGINT` FK | Não | Reserva tributada. |
| `ServiceDate` | `DATE` | Não | Noite abrangida. |
| `RoomID` | `INT` FK | Sim | Quarto associado ao cálculo. |
| `LegalTaxPolicyID` | `INT` FK | Sim | Regra legal aplicável àquela noite, se conhecida. |
| `AppliedTaxPolicyID` | `INT` FK | Sim | Regra interna escolhida no Settings. |
| `GuestCount` | `SMALLINT` | Sim | Número de hóspedes utilizado no cálculo. |
| `TaxablePricePerPerson` | `DECIMAL(12,2)` | Sim | Base tributável por pessoa/noite; não presumir a partir da tarifa que inclui breakfast. |
| `LegalTaxAmount` | `DECIMAL(12,2)` | Sim | Imposto devido segundo regra legal. |
| `ChargedTaxAmount` | `DECIMAL(12,2)` | Sim | Valor realmente cobrado ao hóspede, se conhecido. |
| `CalculationStatus` | `NVARCHAR(24)` | Não | `PendingReview`, `Calculated`, `Confirmed` ou `NotApplicable`. |
| `Notes` | `NVARCHAR(1000)` | Sim | Motivo de ausência de dados, diferença ou ajuste. |
| `CalculatedAt` | `DATETIME2(0)` | Sim | Quando o snapshot foi produzido. |

A diferença legal/cobrado deve ser calculada na aplicação ou num campo computado, sem criar cobranças automáticas duplicadas. Regras financeiras e contabilísticas devem ser validadas antes de entrar em produção.

## `Stay` (nova)

| Campo | Tipo SQL proposto | Nulo | Significado |
|---|---|---|---|
| `StayID` | `BIGINT IDENTITY` PK | Não | Identificador da estadia operacional contínua. |
| `CustomerID` | `BIGINT` FK | Não | Hóspede identificado ao qual se refere a estadia. |
| `StartDate` | `DATE` | Não | Primeira data de chegada da estadia. |
| `EndDate` | `DATE` | Não | Data final de saída, exclusiva para noites. |
| `Notes` | `NVARCHAR(1000)` | Sim | Observações sobre a ligação entre reservas. |
| `CreatedAt` | `DATETIME2(0)` | Não | Data de criação. |

## `StayReservation` (nova)

| Campo | Tipo SQL proposto | Nulo | Significado |
|---|---|---|---|
| `StayID` | `BIGINT` FK, PK composto | Não | Estadia. |
| `ReservationID` | `BIGINT` FK, PK composto | Não | Reserva original associada à estadia. |
| `AddedAt` | `DATETIME2(0)` | Não | Data de associação. |

Este modelo permite que uma mesma reserva familiar participe nas estadias operacionais de hóspedes distintos. Não fundir reservas nem fazer ligação automática só por nome.

## `OTARoomMapping` (nova)

| Campo | Tipo SQL proposto | Nulo | Significado |
|---|---|---|---|
| `OTARoomMappingID` | `INT IDENTITY` PK | Não | Identificador do alias. |
| `OTAID` | `INT` FK | Sim | OTA específica. NULL indica alias genérico. |
| `RoomID` | `INT` FK | Não | Quarto físico interno reconhecido. |
| `ExternalRoomCode` | `NVARCHAR(120)` | Sim | Código externo, quando fornecido. |
| `ExternalRoomName` | `NVARCHAR(255)` | Não | Nome original do quarto no CSV. |
| `NormalizedName` | `NVARCHAR(255)` | Não | Nome normalizado para procura; preservar o texto original. |
| `IsActive` | `BIT` | Não | Se o alias está ativo. |
| `CreatedAt` | `DATETIME2(0)` | Não | Data de criação. |

Implementar validação de colisões: dentro da mesma OTA não podem existir aliases normalizados indistinguíveis que apontem a quartos diferentes. A prioridade de correspondência será código da OTA, depois nome específico da OTA, por fim nome genérico único. Ambiguidade deverá pedir mapeamento manual, nunca escolher ao acaso.

## Dicionário visual dentro do WPF

**Tela `Data Dictionary`**: tabela pesquisável com colunas `Table`, `Field`, `SQL Type`, `Required`, `PK/FK`, `Description`, `Example`, `Version`. À esquerda, categorias/tabelas; à direita, campos e relacionamentos. Filtro por texto, exportação Markdown e etiqueta de estado (`Existing v1.0`, `Proposed v1.1`, `Implemented`). O conteúdo deve ser mantido junto ao código do projeto (JSON de metadados ou catálogo de descrições versionado), e refletir os modelos realmente implementados. Não consultar `INFORMATION_SCHEMA` como única fonte das descrições de negócio.

## Política para a primeira versão

1. Não implementar controlo de disponibilidade ou rejeição por ocupação: o channel manager já o faz.
2. A capacidade do quarto é informativa e útil para o dicionário, não constitui bloqueio no ETL.
3. Política fiscal escolhida por quarto = preferência operacional. Calcular e apresentar legal e cobrado em separado, sem presumir que a regra histórica pode substituir a lei vigente.
4. CSV `詳細` com tarifa da noite = preço **total do quarto**, nunca multiplicar pelo número de pessoas.
5. Breakfast está incluído comercialmente, mas sem informação de base fiscal separada não inferir automaticamente a tributação desse componente.
6. OTA `deposit` permanece `Unclassified` até classificação humana/OTA específica.
7. Criar apenas tabelas definidas em migrations, nunca acrescentar colunas por aparecerem num CSV.

## Implementation status: phases 1 through 5, schema v1.2

The C# model and `InitialSchemaV11` EF Core migration implement the 21 original tables plus the six tables specified in Annex B (27 business tables). EF also maintains its infrastructure table `__EFMigrationsHistory`. The original SQL script remains unchanged and is not executed by the application. Phases 3 and 4 added application screens and read-only CSV preview. Phase 5 now imports the verified combined export supplied as `20261106154030.csv`, with explicit review and additive `ImportSourceAudit` migration; no business tables are added.

- Annex B takes precedence over the earlier recommendation summary: `ValidFrom`/`ValidTo`, separate legal/applied policies and legal/charged tax amounts, `Stay.CustomerID`, and nullable `OTARoomMapping.OTAID` are implemented. `Property`, `OTARoomType`, and additional reservation-group fields are not implemented.
- `Room.Capacity` is a positive `SMALLINT`; `DefaultTaxPolicyID` is nullable. Capacity is informational, with no occupancy or overbooking enforcement.
- All reference keys, money precision, computed charge amounts, filtered import-source uniqueness, one-lead-guest constraint, bed/room composite foreign key and explicit non-cascading relationships are retained. SQL-generated names for original unnamed uniqueness constraints are represented by named unique indexes.
- `vwReservationFinancialSummary` is created and dropped by the migration. Charges and actual payments are aggregated independently by currency; OTA statement items and tax snapshots are excluded.
- New checks validate tax date/range/amount/status values and stay date intervals. Tax brackets have unique lower bounds per policy; preventing overlapping brackets requires service validation before future tax-policy editing is enabled.
- Active OTA-specific room codes and normalized aliases are unique within each OTA. Generic aliases may refer to different rooms and must be treated as ambiguous by the future matcher; duplicate generic aliases for the same room are prevented. Normalized names use Unicode Form KC, collapsed Unicode whitespace and invariant uppercase, with binary SQL collation. Original names remain unchanged.
- Room seeds are embedded from `Gajyun_RoomAliases_seed.json`: five rooms, total capacity 18 and 14 Japanese-labelled dormitory beds. JSON room names (without a space before the parenthesis) are canonical. Private rooms have no bed records and can be assigned with `BedID = NULL`. Aliases include each canonical room name. No OTA-specific attribution is inferred from generic aliases. The malformed `dormit?rio` alias is preserved pending source correction.
- Seeding runs in a serializable transaction with a database application lock. Natural keys prevent duplicate rooms, beds and aliases. Existing capacities, activation states and room tax defaults are preserved; capacity differences produce warnings. Lookup seeds include booking statuses, payment methods, document types and Japan. OTA entries and service prices are not invented.
- Two tax-policy catalog entries are seeded. The 2025 label represents the pre-revision regime (2018-10-01 through 2026-03-01 exclusive); the 2026 regime starts 2026-03-01. `ValidTo` is exclusive. No brackets or room defaults are seeded: staff must verify rates and exemptions before enabling calculations. Reference: https://www.city.kyoto.lg.jp/gyozai/page/0000236942.html . Tax calculation services and collection rules are not enabled in these phases.
- Startup checks connectivity to `master`, detects whether the configured database exists, rejects a populated unmanaged database without EF migration history, applies pending migrations and then seeds data. `MigrateAsync()` creates the database if it is missing; the Windows account needs database-creation permissions. No `EnsureCreated()` or original SQL script is used. Failures never trigger automatic deletion, recreation or migration rollback. Seeding failure rolls back only the seed transaction; completed migrations remain versioned.
- Runtime settings are read from Desktop `appsettings.json`, with an optional override at `%LOCALAPPDATA%\Gajyun\appsettings.json` and `GAJYUN_`-prefixed environment variables (for example `GAJYUN_ConnectionStrings__DefaultConnection`). The connection key is `ConnectionStrings:DefaultConnection`, with default value `Server=(localdb)\MSSQLLocalDB;Database=GuesthouseGajyunDb;Trusted_Connection=True;TrustServerCertificate=True;` (escape the backslash as `\\` in JSON). This is the current Windows user's local SQL Server LocalDB instance, not the `localhost` default SQL Server instance. Hosting configures Microsoft.Extensions.Logging with console/debug output; sensitive-data logging is not enabled. The minimal startup window displays connection and initialization status, with retry/cancel commands.
- EF design-time commands use `GajyunDbContextFactory` with the same default LocalDB connection, or `GAJYUN_DESIGN_CONNECTION`. Generate migrations with `dotnet ef migrations add <Name> --project Gajyun.Data --startup-project Gajyun.Data`. Application startup creates the missing `GuesthouseGajyunDb` database through migrations and then seeds required data; manual database creation in SSMS is unnecessary. Do not execute database-update commands against production to run tests: use an explicitly isolated test database. Reservation updates set `UpdatedAt` in Japan local time; SQL insert timestamp defaults retain `SYSDATETIME()` and the local SQL Server host should use Japan time.
- The MVVM sidebar provides CSV Import, Data Dictionary and Settings. Database connectivity and initialization status remain visible in the footer, including retry and cancellation. The dictionary and CSV file preview do not require a live database.
- The Data Dictionary embeds English structured business metadata in `Gajyun.Data\Metadata\Dictionary.v1.2.json`, translated from this reference. SQL types, nullability, keys, foreign keys, defaults, computed expressions, unique indexes and check constraints come from the compiled EF model, never a live database query. Missing table/field descriptions are explicit errors. All 27 business tables and the financial view are shown; `__EFMigrationsHistory` is infrastructure, not a business table. Proposed but unimplemented entities/fields are not shown. Search works across table and field names, with field details, examples, schema version and relationships. Markdown export is not implemented.
- Settings loads rooms, capacities, the two selectable policy catalog entries, and known original/normalized aliases through `IRoomSettingsService`. Select a room and click Show aliases to refresh its aliases. Saving changes only `Room.DefaultTaxPolicyID` in a serializable transaction; optimistic comparison rejects stale room policy updates and inactive/unknown policies. An explicit no-policy option clears the preference. Historical tax snapshots, charges and legal applicability are untouched. Capacity remains informational and no bed assignment or occupancy enforcement is added.
- CSV Import uses the installed CsvHelper 34 reader API. Select a real file, an optional configured OTA, encoding and delimiter, then preview or reload. Automatic encoding performs strict UTF-8 validation across the complete file, falling back to strict Windows-31J/CP932 decoding when needed. A UTF-8 BOM is recognized; explicit encodings never silently fall back. Auto detection is heuristic and labelled as inferred when there is no BOM; staff should verify Japanese preview text. Comma, semicolon and tab delimiters are explicit choices. Quoting, embedded commas/newlines, Japanese headers and empty optional values are supported. UTF-16, malformed quoting, empty/duplicate headers and oversized records produce visible errors. Record size is limited to 1,000,000 UTF-8 bytes.
- CSV preview runs off the UI thread, supports cancellation/progress, counts all data records and retains only the first 200 for display. Preview counters describe field counts, not reservation validity; inconsistent field counts are flagged and extra fields are retained. The decoded record text preserves quoting and unknown fields (record-terminating line breaks are excluded). Preview alone creates no database records. Import validation separately reads all records from one immutable byte snapshot, limited to 25 MiB and 10,000 rows; exceeding either limit rejects the file rather than truncating it. Guest data and raw CSV payloads are not written to diagnostic logs.
- Generic column selections remain an in-memory foundation, not applied to the verified combined-export profile. OTA is read from each row, not overridden by the optional preview selector. Selecting/reloading a file or changing encoding/delimiter invalidates the prepared review. Validate for import shows every source row and its outcome. Import requires staff acknowledgment that only Ready/Cancelled rows become bookings; Duplicate/Held/Invalid outcomes are audited without overwriting existing bookings or importing invalid/unmapped data. Cancellation and errors are visible; revalidation is required after a failed commit.
- The Core room matcher implements exact OTA code, normalized OTA name and unique generic alias priority. The export's `部屋マスタ` may resolve a known generic alias when the external label is unknown. A disagreement between resolved external/master rooms or any ambiguous alias is held. Import rechecks mappings and active rooms in the transaction. Neither source unit counts nor new labels create physical rooms or enforce occupancy; `部屋記号` is a room-type code, not a bed position. Every imported active booking receives a room-level assignment with `BedID = NULL`; private rooms require no Bed records.
- Automatic tax calculations, tax collection and consecutive-stay suggestions remain deferred. Each imported active night receives a PendingReview tax snapshot with separately recorded date-applicable legal policy and current room preference, but no assumed taxable base or calculated/charged amount. Subsequent room-default changes and repeat imports never rewrite historical snapshots. No names are used to join customers across reservations or create Stays.

### Verified combined export profile and approved decisions

The supplied file has 51 Japanese headers and 340 data records, encoded Windows-31J/CP932. It combines Booking.com (290 rows), Hostelworld (15), Agoda (7) and `予約情報入力(直接販売)` (28); it is not a single-OTA export. Fixed required headers and source-specific verified formats are implemented in `CombinedExportReservationParser`. Unknown OTAs and duplicate OTA/booking keys within a file reject the complete review, without database writes. Other record-level parse/room/price issues are visibly Held; inconsistent CSV field counts are Invalid.

| Source field | Implemented interpretation |
|---|---|
| `予約サイト名` + `予約番号` | OTA identity and external reservation key. Only the four observed source names can initialize source OTA records during a confirmed import. |
| `予約区分` | `予約` and `予約変更` create Confirmed new bookings; `キャンセル` creates Cancelled new bookings without financials or accommodation assignments. This does not authorize updating existing bookings. |
| `予約日時`, `変更日時` | Source booking/modification timestamps, not actual staff check-in. |
| `チェックイン日`, `チェックアウト日`, `泊数` | Required booking interval and matching night count. Active records require exactly one unique rate for every booked night. |
| `宿泊者氏名`, `大人人数`, `子供人数` | Booking-local lead customer and reported total guest count. Names alone never establish shared identity between bookings. Additional source contact fields remain in the audited payload pending agreed customer-update rules. |
| `部屋名称`, `部屋記号`, `部屋マスタ` | Existing-room matching only; never inferred bed identification. |
| `詳細` | Nightly dates, descriptions, JPY total-room prices and optional nightly reported guest counts. One Accommodation charge per night, Quantity 1. No guest/unit multiplication. Full details retained, including unknown suffixes such as `mixed:1`. |
| `連泊情報` | Corroborating nightly room-price schedule, not a customer identity or Stay link. Direct-sale rows may use this verified schedule when nightly details are absent. A disagreement with detailed rates is held. |
| `合計料金` | Must exactly equal the sum of active nightly total-room prices; never treated as proof of payment. |
| `請求料金`, `支払方法`, `部屋料金情報`, `食事条件` and all unknown fields | Retained for audit, not converted into receipts, extra charges or an inferred taxable base. Source meal labels do not establish accommodation-only taxable pricing. |

- **Approved: hold all Agoda records.** `Incl.` versus `RefSell.` pricing remains unconfirmed, including cancelled Agoda records. No Agoda bookings/financials are created by this profile.
- **Approved: new cancelled bookings have no charges.** Their original historical prices/deposits remain only in the source audit; no nightly charges, OTA statements, accommodation assignments or tax snapshots are created.
- Hostelworld `deposit JPY ...` and Booking.com `コミッション(手数料)` labels create Unclassified OTAStatementItems only. These are not actual payments, automatic commission charges, accommodation revenue or deductions from nightly prices. For example three JPY 5,700 nights remain JPY 17,100, even with multiple guests; a JPY 2,052 deposit remains separate and unclassified.
- Measured clean-database review of the supplied sample: **257 active bookings ready, 74 cancelled bookings ready, 9 held, 0 structurally invalid**. Holds comprise all 7 Agoda rows and 2 rows whose detailed nightly rates disagree with their nightly schedule. No conflicting amount is selected automatically. Existing data may change these outcomes to Duplicate or reconciliation holds.
- Existing bookings are located by OTA/external number. A matching previously Processed source fingerprint is Duplicate; all staff edits, payments, extras, groups, notes and customer associations are preserved. Different/unverified source data is Held for explicit reconciliation. This initial importer intentionally does not auto-update, cancel, merge or replace existing bookings. Any changed booking classification between review and commit aborts the transaction.
- All accepted bookings, charges, unclassified statements, guest links and row/batch audits commit in one serializable transaction with a database application lock. Failure rolls back the entire import, never deletes/recreates the database, and never leaves partial business records. Repeat imports add audit history, not duplicate business records.

### Additive v1.2 import audit metadata

Migration `ImportSourceAudit` adds nullable columns so historical rows remain valid, and replaces the reservation-only ImportRow index with `(ReservationID, SourceRowHash)`:

| Table / column | SQL type | Meaning |
|---|---|---|
| `ImportBatch.SourceFileHash` | `varchar(64)` | SHA-256 of exact original bytes, including BOM and line endings. |
| `ImportBatch.SourceEncoding` | `nvarchar(100)` | Effective decoder and inferred-encoding notice where applicable. |
| `ImportBatch.SourceFileContent` | `varbinary(max)` | Exact original file bytes, preserved even if the source file later changes. |
| `ImportBatch.ParserVersion` | `nvarchar(40)` | Version of the verified transformation rules. |
| `ImportRow.SourceRowHash` | `varchar(64)` | SHA-256 of header-sorted original field values for semantic unchanged-row detection; not a substitute for the retained source. |

One ImportBatch is created per OTA represented in a confirmed combined export, preserving the existing single-OTA batch foreign key. Each batch intentionally retains the same complete original combined file. Every data row is audited exactly once per import with its original source record number, raw fields/record text, fingerprint, reason and Processed/Skipped/Failed status. Thus full-file bytes contain personal data from multiple OTAs and are duplicated across their batches: protect the local SQL database, source files and backups with appropriate Windows access controls. Real guest exports are local operational data, not embedded resources or synthetic test fixtures, and should not be published or committed to source control.

Verification uses disposable, GUID-named SQL Server LocalDB databases, never `GuesthouseGajyunDb`, and removes only those exact verification databases. The session-only verifiers passed 104 checks covering CSV/encoding regressions, real sample import/reimport, cancellation exclusions, pending tax, ambiguous deposits, private rooms, staff-data preservation, stale review/mapping rejection and whole-transaction rollback. The full solution builds and the EF migration snapshot has no pending model differences. Persistent xUnit project wiring remains blocked because the required native Visual Studio loaded-project inventory capability is unavailable; these verification programs are not represented as a discovered xUnit suite. The updated ViewModel workflow is verified, but a fresh interactive desktop smoke check was blocked by the local PowerShell script-execution policy; that policy was not changed.
