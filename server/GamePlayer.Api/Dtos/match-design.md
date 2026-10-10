# Match System Design

## Purpose

The match system stores completed game matches and each player's
performance within those matches.

## Match

Represents one completed game match.

Fields:

- Id
- ExternalMatchId
- GameMode
- PlayedAtUtc
- CreatedAtUtc

## MatchParticipant

Represents one player's performance in one match.

Fields:

- Id
- MatchId
- PlayerProfileId
- Score
- Kills
- Deaths
- Result

## Relationships

- One Match has many MatchParticipants.
- One PlayerProfile has many MatchParticipants.
- Match and PlayerProfile therefore have a many-to-many relationship
  through MatchParticipant.

## Business Rules

- ExternalMatchId must be unique.
- A match must contain at least two participants.
- A player can appear only once in the same match.
- Score cannot be negative.
- Kills cannot be negative.
- Deaths cannot be negative.
- Result must be Win, Loss, or Draw.
- Every participant must reference an existing player.