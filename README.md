# Candidate Selection System

A real-time streaming data processing system for candidate selection with reservation policies.

## Features

- **Real-time Processing**: Processes candidate data streams with 100ms intervals
- **Reservation System**: Implements category-based reservations (OBC: 27%, SC/ST: 22%, Women: 33%, etc.)
- **Dual Reservations**: Handles women candidates eligible for multiple categories
- **Dynamic Cutoffs**: Calculates cutoff marks in real-time
- **Live Dashboard**: WebSocket-based real-time updates
- **REST API**: Endpoints for single/batch candidate submission

## Architecture

- **Backend**: .NET Core 8.0 with SignalR
- **Processing**: Channel-based streaming with background service
- **Storage**: In-memory (easily replaceable with SQL Server/PostgreSQL)
- **Frontend**: HTML/JavaScript with SignalR client

## Quick Start

```bash
cd /Users/sarveshkumar/Practice/NetCore/candidate-selection-system
dotnet run
```

Open browser: `http://localhost:5000`

## API Endpoints

- `POST /api/candidate` - Add single candidate
- `POST /api/candidate/batch` - Add multiple candidates

## Configuration

Default reservation percentages in `ReservationConfig`:
- OBC: 27%
- SC/ST: 22% 
- Women: 33%
- Women_OBC: 9%
- Women_SC_ST: 7%
- General: 50%

## Sample Request

```json
{
  "candidateId": "CAND001",
  "candidateName": "John Doe",
  "category": 0,
  "marks": 85.5
}
```

Categories: 0=GENERAL, 1=OBC, 2=SC_ST, 3=WOMAN, 4=WOMAN_OBC, 5=WOMAN_SC_ST