<p align="center">
  <picture>
    <source media="(prefers-color-scheme: dark)" srcset="assets/title-dark.svg">
    <img src="assets/title-light.svg" alt="OnlyShop" width="620">
  </picture>
</p>

<hr>

<p align="center">
  <img src="https://img.shields.io/badge/C%23-F08CB4?style=flat-square&logo=csharp&logoColor=1a1a1a" alt="C#">
  <img src="https://img.shields.io/badge/.NET_10-F08CB4?style=flat-square&logo=dotnet&logoColor=1a1a1a" alt=".NET 10">
  <img src="https://img.shields.io/badge/ASP.NET_Core-F08CB4?style=flat-square&logo=dotnet&logoColor=1a1a1a" alt="ASP.NET Core">
  <img src="https://img.shields.io/badge/Entity_Framework_Core-F08CB4?style=flat-square&logo=dotnet&logoColor=1a1a1a" alt="Entity Framework Core">
  <img src="https://img.shields.io/badge/PostgreSQL-F08CB4?style=flat-square&logo=postgresql&logoColor=1a1a1a" alt="PostgreSQL">
  <img src="https://img.shields.io/badge/React-F08CB4?style=flat-square&logo=react&logoColor=1a1a1a" alt="React">
  <img src="https://img.shields.io/badge/React_Router-F08CB4?style=flat-square&logo=reactrouter&logoColor=1a1a1a" alt="React Router">
  <img src="https://img.shields.io/badge/Vite-F08CB4?style=flat-square&logo=vite&logoColor=1a1a1a" alt="Vite">
  <img src="https://img.shields.io/badge/JavaScript-F08CB4?style=flat-square&logo=javascript&logoColor=1a1a1a" alt="JavaScript">
  <img src="https://img.shields.io/badge/Telegram_Mini_Apps-F08CB4?style=flat-square&logo=telegram&logoColor=1a1a1a" alt="Telegram Mini Apps">
  <img src="https://img.shields.io/badge/Telegram_Bot-F08CB4?style=flat-square&logo=telegram&logoColor=1a1a1a" alt="Telegram Bot">
  <img src="https://img.shields.io/badge/JWT-F08CB4?style=flat-square&logo=jsonwebtokens&logoColor=1a1a1a" alt="JWT">
  <img src="https://img.shields.io/badge/S3_Storage-F08CB4?style=flat-square&logo=amazons3&logoColor=1a1a1a" alt="S3 Storage">
  <img src="https://img.shields.io/badge/Docker-F08CB4?style=flat-square&logo=docker&logoColor=1a1a1a" alt="Docker">
</p>

<p align="center"><b>A clothing store inside Telegram</b></p>

**🌐 Language:** **English** · [Русский](README.ru.md)

> **Status:** portfolio demo. It has not been launched commercially — it is a working solution to a real small-business problem, built as a complete product.

**OnlyShop** is a Telegram Mini App where customers pick an item and a size, add it to a cart and submit an order request on their own. The manager receives the finished order right in Telegram, with buttons to manage its status.

It ships as three parts: a customer-facing Mini App, a web admin panel, and a Telegram bot for managers.

---

## The problem it solves

Small Telegram shops usually sell through direct messages. It looks like this:

> — Hi, is this in stock?
> — Which sizes are left?
> — Do you have other colors?
> — How do I pay?

Every such chat costs the manager time and risks losing the customer, who gets tired of waiting for a reply and leaves. Meanwhile the manager answers the same questions over and over.

**OnlyShop takes the back-and-forth out of the sales funnel:**

| Before (chatting in DMs) | After (OnlyShop) |
|---|---|
| "Do you have size M?" — wait for a reply | The customer sees current sizes and stock on their own |
| The manager writes the order down by hand | The request is placed in the cart in a few taps |
| Customer details have to be pulled out of a conversation | Name, phone, address and delivery method are collected in a form |
| Orders get lost in the message stream | Every order is a separate notification with a status history |

**Result:** higher conversion (the customer reaches the order without waiting for an answer), less routine work for the manager, no lost requests.

The core idea: **a small shop takes orders through a cart and request form inside Telegram instead of chatting in DMs.**

---

## How it works

1. The customer opens the Mini App from the bot and browses the catalog.
2. They open a product page, choose a size and add it to the cart.
3. They submit a request: contact details, delivery (courier or pickup), payment method, comment.
4. **The manager gets a new-order notification in Telegram** with the items, total, customer details and status buttons.
5. For online payment, the customer sends the bot a screenshot of the receipt. The bot forwards it to managers with "Confirm" / "Reject" buttons.
6. The manager moves the order through its statuses, and the customer is notified of changes.

### Order lifecycle

```
Pending → Confirmed → Paid → Delivering → Completed
   └─────────┴─────────┴──→ Canceled
```

Allowed transitions are enforced in the domain model (`Order.CanTransitionTo`); invalid ones are rejected. When an order is canceled, stock is returned to inventory and any spent bonuses go back to the customer.

---

## Features

### Customer Mini App
- Catalog with categories and search
- Product page: photo gallery, size selection based on stock, item condition, brand
- Cart, checkout, order history, order cancellation
- Favorites
- Currency switch (BYN / RUB)
- Bonus program: points earned on completed orders, which can be spent on later ones
- Pre-orders (presale) with a closing date
- Contact the manager, delivery information
- Clean design with smooth animations, Telegram haptic feedback and native Back button support

### Admin panel (`/admin`)
- Products: create, edit, photos, prices, publish and archive
- **Sizes and stock**: per-size product variants, stock adjustments
- Orders: filters, details, status changes
- Categories, presale locations, staff
- `Admin` and `Manager` roles
- Linking a staff member's Telegram account via a one-time link

### Telegram bot for managers
- New-order notification that updates as the status changes
- Inline buttons: ✅ Confirm · 💳 Paid · 🚚 Delivering · 🏁 Completed · ❌ Cancel
- Receives payment screenshots and confirms the receipt in one click
- Runs on a webhook

---

## Tech stack

| Layer | Stack |
|---|---|
| Backend | C# / ASP.NET Core (.NET 10), Minimal API |
| Database | PostgreSQL, Entity Framework Core (migrations applied on startup) |
| Customer Mini App | React 18 + htm (no build step), Telegram WebApp SDK |
| Admin panel | React 18, React Router, Vite |
| Telegram | Telegram.Bot, webhook |
| Authentication | Mini App: Telegram `initData` signature validation; admin: JWT |
| Photo storage | S3-compatible storage (AWS SDK) |
| Deployment | Docker (multi-stage) |

### Architecture

The backend is layered (Clean Architecture):

```
OnlyShop.Core            — entities, enums, domain rules (Order, Product, ProductVariant…)
OnlyShop.Application     — services, DTOs, repository interfaces, order business logic
OnlyShop.Infrastructure  — EF Core, PostgreSQL, S3, JWT, Telegram client
OnlyShop.Adapters        — HTTP endpoints, Telegram webhook, bot handler
OnlyShop                 — entry point, static files (Mini App and admin), Dockerfile
```

Dependencies point inward: the domain layer knows nothing about infrastructure.

---

## Repository structure

```
OnlyShop/
├── ClientApp/        customer Mini App sources (js, css)
├── AdminApp/         admin panel sources (React + Vite)
├── wwwroot/          served static files (built admin, Mini App)
└── Program.cs

OnlyShop.Core/ · OnlyShop.Application/ · OnlyShop.Infrastructure/ · OnlyShop.Adapters/
Dockerfile
```

---

## Running locally

**Requirements:** .NET 10 SDK, PostgreSQL, Node.js (for the admin panel), a Telegram bot from @BotFather.

1. Clone the repository and set up configuration (for example via user-secrets or `appsettings.Development.json`):

   | Section | Purpose |
   |---|---|
   | `ConnectionStrings:Default` | PostgreSQL connection |
   | `Jwt` (`Issuer`, `Audience`, `Key`) | admin tokens |
   | `Telegram:BotToken` | bot token |
   | `TelegramWebhook` (`PublicBaseUrl`, `SecretToken`, `MiniAppUrl`, `BotUsername`) | webhook and Mini App link |
   | `FileStorage` (`S3Endpoint`, `S3AccessKey`, `S3SecretKey`, `S3BucketName`, `S3Region`) | photo storage |
   | `Bonus` (`AccrualPercent`, `MaxSpendPercent`) | bonus program (defaults: 5% / 30%) |

2. Run the backend (migrations are applied automatically):

   ```bash
   dotnet run --project OnlyShop
   ```

   The service listens on `http://0.0.0.0:5201`.

3. Admin panel in development mode:

   ```bash
   cd OnlyShop/AdminApp
   npm install
   npm run dev      # http://localhost:5174, /api is proxied to the backend
   ```

   The production build (`npm run build`) goes to `OnlyShop/wwwroot/admin` and is served by the backend at `/admin`.

4. Mini App: open `/` inside Telegram. Outside Telegram (or with the `?mock` parameter) the app runs on mock data, so you can look at the UI without a bot.

### Docker

```bash
docker build -t onlyshop .
docker run -p 8080:8080 onlyshop   # plus environment variables for configuration
```

---

## What's not here

The project is deliberately a demo: there is no built-in payment processing (payment is confirmed by the manager from a receipt), no automated tests, and it has never run in commercial operation.

---

## Author

Built as a portfolio project: from analyzing the business problem to the backend, the interface and the bot.
