# Very simple STUN client

[![forthebadge](https://forthebadge.com/images/badges/0-percent-optimized.svg)](https://forthebadge.com)
[![forthebadge](https://forthebadge.com/images/badges/contains-technical-debt.svg)](https://forthebadge.com)

As the name suggests, this is a very simple and **unfinished** STUN client written in C# and based on [RFC8489](https://tools.ietf.org/html/rfc8489) that can run over TCP or UDP.

This was made as a proof of concept with the intent to add NAT punching to [Mirror networking](https://mirror-networking.com/) using the STUN protocol.

It only implements the `XOR-MAPPED-ADDRESS`, `MAPPED-ADDRESS`, and `SOFTWARE` attributes, these are enough to request an IP endpoint to a public STUN server.

# How to use

_Please don't_
