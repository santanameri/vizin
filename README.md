# Sistema de Locação
Este projeto é uma API robusta para gerenciamento de locação de imóveis, desenvolvida por Lis Marreiros
e Merielly Santana. O sistema permite que **Anfitriões** anunciem propriedades e **Hóspedes** realizem reservas,
pagamentos e avaliações.

## Funcionalidades Principais

- **Gestão de Usuários:** Cadastro e autenticação de Hóspedes e Anfitriões.
- **Anúncios:** Cadastro, edição, filtragem e atualização de valores de diárias de imóveis.
- **Reservas:** Criação de reservas com datas específicas e verificação de disponibilidade.
- **Comodidades:** Adição e remoção de itens como Wi-Fi, Piscina, etc., aos imóveis.
- **Pagamentos:** Gestão do fluxo de pagamento de reservas.
- **Favoritos:** Sistema para salvar e remover imóveis de interesse.
- **Avaliações:** Feedback detalhado sobre estadias e experiências.

## Tecnologias Utilizadas

- **Linguagem:** C#
- **Framework:** ASP.NET Core (Web API)
- **Autenticação:** JWT (JSON Web Token)
- **Arquitetura:** Baseada em Services e DTOs (Data Transfer Objects)

## Estrutura de Dados (Modelo ER)

A API baseia-se nas seguintes entidades principais:

- **User:** Gerencia perfis e credenciais.
- **Property:** Detalhes do imóvel e valores.
- **Booking:** Controle de status (Pending, Confirmed, Cancelled) e períodos de estadia.
- **Payment:** Registra métodos de pagamento e status da transação.
- **Favorite** Favorita imóveis
- **Review:** Notas e comentários sobre as locações.

## Endpoints da API

A URL base da aplicação é `http://localhost:5066/`.

### 🔐 Autenticação e Usuário (`/user`)

- `POST /user`: Cadastra um novo usuário (Hóspede/Anfitrião).
- `POST /user/login`: Autentica e retorna o Token JWT.
- `GET /user/{userId}`: Recupera informações do perfil.

### 🏠 Imóveis (`/property`)

- `GET /property`: Lista todos os imóveis (Hóspedes).
- `GET /property/my`: Lista todos os imóveis do anfitrião logado.
- `POST /property`: Cadastra um novo imóvel (Apenas Anfitriões).
- `PATCH /property/{propertyId}`: Atualiza o valor da diária.
- `PUT /property/{propertyId}`: Atualiza o imóvel.
- `GET /property/amenities`: Lista todas as comodidades.
- `POST /property/add-amenity/{amnenityId}`: Adiciona comodidades ao imóvel.
- `GET /property/filter?estado={estado}`: Filtra por endereço.
- `GET /property/filter-amenities/?amenityIds={amenityId}`: Filtra por comodidade.
- `GET /bookings/availability/?checkIn=0000-00-00&checkOut=0000-00-00`: Verificar disponibilidade - pode ser um sub-recurso ou query em properties)

### 📅 Reservas (`/booking`)

- `POST /booking/{propertyId}/book`: Cria uma nova reserva.
- `GET /booking/my-bookings`: Lista o histórico de reservas do usuário logado.
- `PATCH /booking/{bookingId}/cancel`: Cancela uma reserva existente.
- `GET /booking/report`: Gera relatório em CSV (Apenas Anfitriões).

### 💳 Pagamentos e Extras

- `POST /payment/{bookingId}/pay`: Processa o pagamento da reserva.
- `POST /favorite/toggle`: Adiciona/Remove imóvel dos favoritos.
- `POST /review/{bookingId}`: Registra uma avaliação após a estadia.
- `DELETE /review/{reviewId}`: Apaga a review cadastrada (Hóspede).

## Regras de Negócio

- Apenas o dono do imóvel pode criá-lo e editá-lo
- Capacidade do imóvel deve ser maior que zero
- Valor da diária nunca pode ser zero ou negativo
- **Políticas de Acesso:** Certas rotas são restritas a Anfitriões (`AnfitriaoOnly`) ou Hóspedes (`HospedeOnly`) via atributos de autorização do ASP.NET.

### Como executar

1. Clone o repositório.
2. Certifique-se de ter o SDK do .NET instalado.
3. Configure sua string de conexão no `appsettings.json`.
4. Execute `dotnet run`.
5. Utilize o **Insomnia** ou **Postman** para testar os endpoints utilizando o Bearer Token obtido no login.