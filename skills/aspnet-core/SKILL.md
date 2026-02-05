---
name: aspnet-core
display_name: ASP.NET Core (C#) Production Backend
description: >
  Production-grade best practices and architectural rules for building
  ASP.NET Core Web APIs in C#, combining Microsoft official guidance
  and disciplined agent behaviors inspired by Antigravity Kit.
version: 1.0.0
language: csharp
framework: aspnet-core
tags:
  - csharp
  - dotnet
  - aspnet-core
  - web-api
  - backend
  - production
triggers:
  - c#
  - csharp
  - .net
  - asp.net
  - asp.net core
  - web api
  - minimal api
---

## 🎯 Purpose

This skill teaches the agent to behave like a **senior ASP.NET Core backend engineer**
when designing, writing, refactoring, or reviewing C# Web APIs.

The agent must prioritize:
- Maintainability
- Clear architecture boundaries
- Production safety
- Long-term scalability

---

## 🧠 Agent Behavior Rules (Antigravity-style)

When this skill is active, the agent MUST:

1. **Think before coding**
   - Clarify architecture intent
   - Identify application boundaries
   - Avoid jumping directly into implementation if context is missing

2. **Prefer structure over shortcuts**
   - Reject “quick hacks”
   - Avoid putting logic where it does not belong

3. **Explain critical decisions briefly**
   - Especially when choosing patterns or abstractions

---

## 🧱 Architecture Rules

### Required Architecture

Use **Layered / Clean Architecture**:

