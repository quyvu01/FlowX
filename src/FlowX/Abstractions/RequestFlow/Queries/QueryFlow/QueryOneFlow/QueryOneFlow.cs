﻿using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using FlowX.Errors;

namespace FlowX.Abstractions.RequestFlow.Queries.QueryFlow.QueryOneFlow;

public class QueryOneFlow<TModel, TResponse> :
    IQueryOneFilter<TModel, TResponse>,
    IQueryOneSpecialAction<TModel, TResponse>,
    IQueryOneMapResponse<TModel, TResponse>,
    IQueryOneErrorDetail<TModel, TResponse>,
    IQueryOneFlowBuilder<TModel, TResponse> where TModel : class where TResponse : class
{
    public QuerySpecialActionType QuerySpecialActionType { get; private set; }
    public Expression<Func<TModel, bool>> Filter { get; private set; }
    public Func<IQueryable<TModel>, IQueryable<TModel>> SpecialAction { get; private set; }
    public Func<IQueryable<TModel>, IQueryable<TResponse>> SpecialActionToResponse { get; private set; }
    public Func<TModel, TResponse> MapFunc { get; private set; }
    public Error Error { get; private set; }

    public IQueryOneSpecialAction<TModel, TResponse> WithFilter(Expression<Func<TModel, bool>> filter)
    {
        Filter = filter;
        return this;
    }

    IQueryOneMapResponse<TModel, TResponse> IQueryOneSpecialAction<TModel, TResponse>.WithSpecialAction(
        Func<IQueryable<TModel>, IQueryable<TModel>> specialAction)
    {
        QuerySpecialActionType = QuerySpecialActionType.ToModel;
        SpecialAction = specialAction;
        return this;
    }

    public IQueryOneErrorDetail<TModel, TResponse> WithSpecialAction(
        Func<IQueryable<TModel>, IQueryable<TResponse>> specialAction)
    {
        QuerySpecialActionType = QuerySpecialActionType.ToTarget;
        SpecialActionToResponse = specialAction;
        return this;
    }

    public IQueryOneFlowBuilder<TModel, TResponse> WithErrorIfNull([NotNull] Error error)
    {
        Error = error;
        return this;
    }

    public IQueryOneErrorDetail<TModel, TResponse> WithMap(Func<TModel, TResponse> mapFunc)
    {
        MapFunc = mapFunc;
        return this;
    }
}