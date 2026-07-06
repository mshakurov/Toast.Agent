# Toast.Agent Architecture

## Purpose

Toast.Agent is a lightweight Android background agent written in C#.

Responsibilities:

- poll server
- execute commands
- show messages
- send execution results
- work reliably on Android TV devices

---

## Projects

Toast.Agent.Android

Android specific code only.

Responsibilities:

- ForegroundService
- Activities
- Receivers
- Notifications

Toast.Core

Platform independent.

Responsibilities:

- Polling
- Commands
- Logging
- Settings
- Networking

---

## Main Components

Agent

PollingEngine

CommandProcessor

Logger

Settings

NetworkClient

UserNotifier