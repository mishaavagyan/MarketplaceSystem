# 🛒 Marketplace System

## 🚀 Overview
Marketplace backend system built with ASP.NET Core using a microservices-oriented architecture.  
The project demonstrates real-world backend patterns such as authentication, asynchronous communication, caching, and background processing.

---

## 🧱 Architecture

The system is split into multiple services:

- **AuthService (API)** – Handles user registration, login, and verification
- **UserService (API + Worker)** – Manages user profiles and listens to events from RabbitMQ
- **PhoneCatalogService (API)** – Manages product catalog
- **OrderService (API)** – Handles order creation and updates
- **NotificationService (Worker)** – Sends notifications (e.g. SMS) based on events

Communication between services is done using **RabbitMQ (event-driven architecture)**.

---

## ⚙️ Features

- 🔐 User registration & authentication(JWT)
- 👤 Automatic profile creation via async events
- 🛒 Order creation and update system
- 📩 Notification system (SMS via background worker)
- ⚡ Redis-based verification code storage
- 🧵 Background processing using Worker Services
- 📊 Optimized queries using IQueryable
- 🧾 Logging across services

---

## 🧰 Tech Stack

- **Language:** C#
- **Framework:** ASP.NET Core Web API
- **Database:** MSSQL
- **ORM:** Entity Framework Core
- **Caching:** Redis (used for verification codes)
- **Message Broker:** RabbitMQ
- **Architecture:** Microservices + Event-driven
- **Other:** Dependency Injection, Logging

---

## 🔄 How It Works

- When a user registers → AuthService publishes an event → UserService creates profile  
- When an order is created/updated → OrderService publishes event → NotificationService sends SMS  
- Verification codes are stored in Redis and validated during authentication  

---

## 📌 What I Learned

- Designing scalable backend systems  
- Working with microservices and async communication  
- Using Redis for caching and temporary data  
- Implementing message brokers (RabbitMQ)  
- Building production-like backend architecture  

---

## 📷 Future Improvements

- Add API Gateway  
- Dockerize services  
- Add centralized logging (e.g. ELK)  
- Implement role-based authorization  
