# ECommerce-AI

## Functionality

- **Product Browsing**: Users can view a list of available products.
- **Shopping Cart Interaction**: Users can add products to their cart, update quantities, and remove items before proceeding to checkout.
- **Order Creation**: Users can place orders based on the contents of their shopping cart.
- **Payments**: Secure payment processing with Stripe.

## Setting Up 

### Prerequisites

- Git installed on your machine
- Docker Desktop installed
- Aspire installed

### Step-by-Step Guide

### 1. Clone the Repository

```bash
git clone https://github.com/DaniloChaku/ECommerce-AI.git
cd ECommerce-AI
```

### 2. Set Up Environment Variables

Open the `appsettings.json` file in ECommerce.AppHost project and fill in the required values.

### 3. Build and Run with Aspire

Once your environment variables are configured, start the application using Aspire:

```bash
dotnet run --project src/ECommerce.AppHost
```

### 4. Access The Application

Once everything is running, you can access the application at the configured port (default is http://localhost:5001).  
API documentation (Swagger UI) is typically available at http://localhost:8080/swagger

## Quality Analysis
Link: https://sonarcloud.io/summary/overall?id=danilochaku_ecommerce&branch=main.

## AI Task Completion Feedback
- Was it easy to complete the task using **AI**?
  
  It was easier than writing it from scratch, but it's important to use AI cautiously when working on projects of this size or larger. AI lacks full project context and tends to generate suboptimal or incorrect code as complexity increases. 

- How long did task take you to complete? (*Please be honest, we need it to gather anonymized statistics*)
  
  8 hours. It would have taken an hour less if I hadn’t encountered a strange issue where endpoints with the [Authorize] attribute were returning 404. This turned out to be caused by using AddIdentity instead of AddIdentityCore.

- Was the code ready to run after generation? What did you have to change to make it usable?
  
  When working with Stripe, both ChatGPT and Claude suggested classes and methods that didn’t exist. I also had to refactor the generated code to improve quality — for example, by splitting large methods into smaller ones, applying the options pattern for configuration, and replacing magic literals with named constants.

- Which challenges did you face during completion of the task?
  
  A major challenge was debugging and correcting issues introduced by the AI due to its limited understanding of the full codebase context. I also had to spend time improving and refactoring the code for maintainability. As the codebase grew, the AI started to write really bad code and it would take me a long time if I decided to refactor everything.
  
- Which specific prompts you learned as a good practice to complete the task?
  
  Write unit tests with [framework and libraries]. Use [naming conventions] naming conventions.
