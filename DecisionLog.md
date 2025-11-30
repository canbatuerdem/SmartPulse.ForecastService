# Architectural & Design Decisions

1. **.NET Version Selection (.NET 8 vs .NET 10)**  
   Although newer versions of .NET are available, .NET 8 is a Long-Term Support (LTS) release, which provides:
   - Long-term security updates  
   - Higher ecosystem stability  
   - Broad compatibility with production-grade libraries  
   - Lower operational risk for enterprise environments  

   Since SmartPulse already uses .NET as its primary backend technology, the goal was not to experiment with the newest runtime features, but rather to demonstrate a stable, production-ready, and low-risk architecture aligned with real-world enterprise standards.

2. **ORM & Temporal Data Strategy**  
   Entity Framework Core was selected to keep the solution simple, accessible, and aligned with the default .NET ecosystem tooling. Although in large-scale production environments NHibernate might be preferred for its advanced mapping control and performance tuning capabilities, EF Core was a more pragmatic choice for this assignment to reduce unnecessary abstraction and setup overhead. SQL Server temporal tables were deliberately used to demonstrate native system-versioned history handling with minimal custom logic, showcasing a production-grade feature in a lightweight and elegant way without introducing additional audit or versioning complexity.

3. **Database Selection (Microsoft SQL Server)**  
   Microsoft SQL Server was selected due to its seamless integration with the .NET ecosystem, native support for system-versioned temporal tables which perfectly fits the forecast history requirement, and its strong ACID guarantees for consistent aggregation workloads such as company position calculation. In addition, it is a mature, enterprise-proven platform with rich operational tooling, and it is the database where I have the deepest practical and near administrative-level experience, allowing the solution to focus on architectural quality and reliability rather than database experimentation.

4. **Database Indexing & Performance Optimization Strategy**  
   I intentionally kept database optimizations minimal and only added the essential unique index on `(PowerPlantId, ForecastHourUtc)` to enforce idempotency and data consistency. Since the expected data volume and access patterns are not specified, adding extra indexes, views, or advanced tuning at this stage would be premature optimization. If the system evolves into a high-volume scenario (hundreds of millions or billions of rows), then a proper performance phase would include a full indexing strategy, partitioning, and tuning of factors such as fill factor and storage layout based on real workload characteristics.

5. **Transactions and Event Publishing**  
   All database operations in this service are single-command operations (single insert or update followed by a single save). EF Core already executes `SaveChanges` within an implicit transaction, therefore no explicit database transaction was required.

   For the database + message queue dual-write scenario, a distributed transaction could be used, but this is considered heavy and generally discouraged in modern microservice architectures.

   Instead, in a real production system, the Outbox Pattern would be the preferred solution:
   - The forecast update and the outgoing event would be stored in the same database transaction.  
   - A background worker would later publish the outbox records to the message broker.  
   - This would guarantee eventual consistency and exactly-once delivery semantics.  

   For this assignment, to keep the design simple and focused, the event is published after a successful database commit, and the outbox pattern was intentionally not implemented.

6. **Exception Handling Strategy**  
   A minimal exception-handling implementation exists in `ForecastService.PublishPositionChangedAsync`. At this point, the classic *Swallow vs. Rethrow* design discussion arises:
   - Swallowing the exception may keep the API responsive but risks losing the event.  
   - Rethrowing would make the failure visible but could impact API availability.  

   A full production-grade solution would require:
   - Retry policies  
   - Dead-letter queues  
   - Proper correlation logging  
   - Possibly an outbox with guaranteed delivery  

   Since none of these were required by the assignment, only a minimal implementation was added to demonstrate awareness of the problem.

7. **Logging Strategy**  
   Different types of logging (audit, performance, and error logging) were evaluated. To avoid unnecessary complexity, only minimal structured application logging was implemented inside `ForecastService` as a demonstration.

   - **Audit Logging:** Skipped. Without authentication or user identity, audit logs would be meaningless.  
   - **Performance Logging:** Skipped. Proper performance logging would require counters, metrics, and a monitoring stack (e.g., Prometheus, Application Insights), which is outside the scope of this assignment.  
   - **Error Logging:** A minimal implementation is provided around the event publishing logic. A full production-grade implementation would require dumping request context, correlated IDs, SQL commands, and integration with systems such as ELK or Splunk, which would add a full observability layer to the project.  

   This approach demonstrates awareness of logging requirements without over-engineering the solution.

8. **Authentication & Authorization**  
   No authentication or authorization mechanism was implemented, as it was not part of the assignment requirements. To keep the solution focused on the core forecasting and eventing workflow, all security concerns were intentionally excluded. Introducing authentication would require additional infrastructure (identity provider, token handling, role model), which would significantly increase the scope without adding direct value to the core problem.

9. **Event Publishing Layer Placement**  
   The event publishing mechanism was intentionally not separated into an additional dedicated layer to avoid unnecessary structural complexity for the scope of this assignment. Instead, it was placed in the same assembly as the persistence layer (Repository), which effectively acts as the Infrastructure layer in this architecture. Since both database access and message broker integration are infrastructure concerns, co-locating them preserves a clear separation between business logic (Service) and external technical dependencies, while keeping the overall solution simple, readable, and easy to maintain without introducing extra project boundaries.

10. **DTO Mapping Strategy (Without AutoMapper)**  
    Although AutoMapper is a well-known and widely used library for object mapping in .NET ecosystems, it was intentionally not introduced in this solution. Given the very limited number of DTOs and the simplicity of the mapping logic, using AutoMapper would add unnecessary abstraction, configuration, and runtime overhead. All mappings were implemented explicitly to keep the data flow fully readable, debuggable, and predictable. For larger-scale systems with complex object graphs, AutoMapper would be a valid choice; however, for this assignment, explicit mapping provides a cleaner and more transparent solution without introducing additional moving parts.

11. **Primary Key Generation Strategy**  
    All primary keys are generated on the application side using GUIDs instead of relying on database-generated identities. This approach ensures that entity identifiers are available immediately at creation time, enables their use in logs, events, and correlation scenarios before persistence, and avoids tight coupling to a specific database engine’s identity mechanisms. Application-side key generation is also more suitable for distributed and event-driven architectures, where entities may need globally unique identifiers prior to being persisted.

12. **Database Initialization & Test-Only Bootstrapping**  
    Automatic database creation, migration, and seeding are triggered from the API host on startup purely as a test/demo convenience, so a fresh SQL Server instance can be brought to a usable state with a single `docker compose up`. This is also the only reason the API layer has a direct reference to the repository/infrastructure project and knows about the `ForecastDbContext`, which is a deliberate layering violation for the sake of simplicity in this assignment. In a real production environment, database lifecycle and schema changes would be handled by separate deployment pipelines or migration jobs, and the API would not depend on infrastructure types. Likewise, the `PowerPlant` data would typically be owned by a separate microservice rather than being seeded by this service.

13. **Timestamp Normalization Strategy**  
    Incoming forecast timestamps are automatically normalized to the nearest whole hour by truncating minute and second components instead of rejecting the request with a validation error. This design was chosen to keep the system tolerant to imperfect client input while ensuring that all persisted data remains aligned on a consistent hourly time grid. The goal is to preserve clean, testable, and deterministic data without forcing strict client-side time formatting during integration and testing phases.
