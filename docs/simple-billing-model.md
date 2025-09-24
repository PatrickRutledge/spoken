# Simple Premium AI Paragraph Breaks - Billing Model

## Overview
Simple pay-per-use model where clients pay for AI-enhanced paragraph breaking using online models.

## Billing Options

### Option 1: Pay-Per-Analysis (Recommended)
- **Client Cost**: $0.10 per AI analysis
- **Our Cost**: ~$0.02 per OpenAI API call  
- **Profit**: $0.08 per analysis (400% markup)
- **Implementation**: Stripe payment + usage tracking

### Option 2: Credit Packs
- **$5 Pack**: 50 analyses ($0.10 each)
- **$10 Pack**: 120 analyses ($0.083 each - 17% discount)
- **$20 Pack**: 250 analyses ($0.08 each - 20% discount)

### Option 3: Simple Subscription
- **Free Tier**: 5 AI analyses per month
- **Premium**: $4.99/month for 50 analyses
- **Pro**: $9.99/month for unlimited analyses

## Transaction Flow

```
1. User clicks "AI Enhanced Paragraphs" 
2. Check if user has credits/subscription
3. If not, prompt for payment (Stripe)
4. Process payment → Add credits to account
5. Make OpenAI API call
6. Deduct credit from user account
7. Return enhanced paragraph breaks
```

## Technical Implementation

### Payment Processing: Stripe
- Simple integration for one-time payments
- Credit card processing
- Webhook for payment confirmation
- Automatic credit addition

### Usage Tracking
- Database table: user_id, credits_remaining, last_used
- Deduct 1 credit per AI analysis
- Block usage when credits = 0

### Cost Analysis
- OpenAI GPT-4o-mini: ~$0.002 per 1K tokens
- Average biblical passage: ~500 tokens input + 100 tokens output = 600 tokens
- Cost per analysis: ~$0.0012
- Charge $0.10 → 8,300% markup (very profitable)

## Implementation Plan

1. **Add Stripe payment integration**
2. **Create credits database table**
3. **Add "Buy Credits" UI**
4. **Add credit check before AI calls**
5. **Track usage and billing**

This keeps it simple while being profitable!