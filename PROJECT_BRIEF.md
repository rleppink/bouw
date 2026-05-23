An orchestrator that drives an opinionated software-building workflow, calling LLMs only at the steps where they belong and using deterministic code for everything else.

It is for a solo software engineer who has hit the limits of LLM-as-orchestrator and wants deterministic process around the parts LLMs are actually good at.

For the MVP, shipping speed wins: an opinionated single path with human checkpoints beats configurable workflows or fully autonomous agents.

It is not a general-purpose agent framework: every supported workflow must be in service of shipping software.

LLMs are treated as stateless functions, not orchestrators: code assembles the context for each call, sends one prompt, receives one response, and control flow stays entirely in code.
