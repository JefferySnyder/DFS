# Next Steps

- Replication: Change the Coordinator to assign each block to multiple storage nodes (e.g., Primary and Secondary) to ensure data redundancy.
- Heartbeats: Implement a background thread (IHostedService) in the Coordinator that pings storage nodes every few seconds to remove dead nodes from the pool automatically.
- gRPC Streaming: For heavy block transfers, swap out HTTP/REST web endpoints for gRPC, which uses HTTP/2 streaming for lower overhead and faster data throughput in .NET.
